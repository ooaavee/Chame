using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chame.Middlewares
{
    /// <summary>
    /// A middleware for loading content files.
    /// </summary>
    internal sealed class ContentLoaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<ContentLoaderOptions> _options;
        private readonly ILogger<ContentLoaderMiddleware> _logger;

        /// <summary>
        /// All valid request paths and corresponding content-types.
        /// </summary>
        private readonly IDictionary<string, IContentInfo> _paths;

        public ContentLoaderMiddleware(RequestDelegate next, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger)
        {
            _next = next;
            _options = options;
            _paths = GetValidPaths(options.Value.ContentModel, options.Value.RequestPathTemplate, logger);
            _logger = logger;
        }

        /// <summary>
        /// Resolves valid request paths for this middleware.
        /// </summary>
        private static IDictionary<string, IContentInfo> GetValidPaths(IContentModel contentModel, string requestTemplatePath, ILogger logger)
        {
            var paths = new Dictionary<string, IContentInfo>();

            foreach (var content in contentModel.SupportedContent)
            {
                var path = string.Format(requestTemplatePath, content.Extension).ToLower(CultureInfo.InvariantCulture);
                paths.Add(path, content);
            }

            var msg = new StringBuilder();
            msg.AppendLine("Middleware initialized and listening following paths:");
            foreach (var path in paths.Keys)
            {
                msg.AppendLine($" {path}");
            }
            logger.LogInformation(msg.ToString());

            return paths;
        }

        public async Task Invoke(HttpContext http)
        {
            if (IsValidRequest(http, out ContentLoadingContext context, out List<IContentLoader> loaders))
            {
                var responses = await ContentLoadingUtility.LoadContentAsync(context, loaders);
                if (responses.Any())
                {
                    await WriteResponseAsync(context, responses);
                    return;
                }
            }

            await _next(http);
        }

        private bool IsValidRequest(HttpContext http, out ContentLoadingContext context, out List<IContentLoader> loaders)
        {
            context = null;
            loaders = null;

            // must be HTTP GET
            if (http.Request.Method != HttpMethods.Get)
            {
                return false;
            }

            if (!http.Request.Path.HasValue)
            {
                return false;
            }

            // check that request path is valid
            var path = http.Request.Path.ToString().ToLower(CultureInfo.InvariantCulture);
            if (!_paths.TryGetValue(path, out IContentInfo info))
            {
                return false;
            }

            _logger.LogInformation(string.Format("Started to handle a HTTP request [path = {0}].", http.Request.Path.ToString()));

            // an optional filter
            var filter = http.Request.Query["filter"].FirstOrDefault();

            // content loaders 
            loaders = ContentLoadingUtility.GetContentLoaders(http, info);
            if (!loaders.Any())
            {
                return false;
            }

            // HTTP ETag
            string eTag = null;
            if (_options.Value.SupportETag)
            {
                var eTagFound = HttpETagHelper.TryParse(http.Request, out eTag);
                if (eTagFound && loaders.Count > 1)
                {
                    eTag = null;
                    _logger.LogDebug("HTTP ETag will be disabled for this request, because there are multiple content loaders for this request.");
                }
            }

            // theme
            var theme = GetTheme(http);
            if (theme == null)
            {
                _logger.LogCritical("Could not resolve theme.");
                return false;
            }

            _logger.LogInformation($"A theme '{theme.GetName()}' will be used.");

            context = new ContentLoadingContext(http, info, theme, filter, eTag);

            return true;
        }

        private async Task WriteResponseAsync(ContentLoadingContext context, IList<ContentLoaderResponse> responses)
        {
            ContentLoaderResponse response = ContentLoadingUtility.Bundle(context, responses);

            context.HttpContext.Response.ContentType = context.ContentInfo.MimeType;

            // ok
            if (response.Status == ResponseStatus.Ok)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

                if (!string.IsNullOrEmpty(response.ETag) && _options.Value.SupportETag && responses.Count == 1)
                {
                    HttpETagHelper.Use(context.HttpContext.Response, response.ETag);
                }

                await context.HttpContext.Response.Body.WriteAsync(response.Data, 0, response.Data.Length);
            }

            // not modified
            else if (response.Status == ResponseStatus.NotModified)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
            }
        }

        private ITheme GetTheme(HttpContext http)
        {
            ITheme theme = null;
            IThemeResolver resolver = http.RequestServices.GetService<IThemeResolver>();
            if (resolver != null)
            {
                theme = resolver.GetTheme(http);
            }
            return theme ?? _options.Value.DefaultTheme;
        }

    }
}
