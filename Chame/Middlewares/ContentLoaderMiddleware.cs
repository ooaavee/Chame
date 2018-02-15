using Chame.ContentLoaders;
using Chame.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Chame.Middlewares
{
    /// <summary>
    /// A middleware for loading content files.
    /// </summary>
    internal sealed class ContentLoaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ChameUtility _utils;
        private readonly IOptions<ContentLoaderOptions> _options;
        private readonly ILogger<ContentLoaderMiddleware> _logger;

        /// <summary>
        /// All valid request paths and corresponding content-types.
        /// </summary>
        private readonly IDictionary<string, IContentInfo> _paths = new Dictionary<string, IContentInfo>();

        public ContentLoaderMiddleware(RequestDelegate next, IOptions<ContentLoaderOptions> options, ChameUtility utils, ILogger<ContentLoaderMiddleware> logger)
        {
            _next = next;
            _utils = utils;
            _options = options;
            _logger = logger;

            foreach (IContentInfo info in _options.Value.ContentModel.SupportedContent)
            {
                string path = string.Format(_options.Value.RequestPathTemplate, info.Extension).ToLower(CultureInfo.InvariantCulture);
                _paths.Add(path, info);
            }
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (IsValidRequest(httpContext, out ContentLoadingContext context, out List<IContentLoader> loaders))
            {
                var responses = await _utils.LoadContentAsync(context, loaders);

                if (responses.Any())
                {
                    await WriteResponseAsync(context, responses);
                    return;
                }
            }

            await _next(httpContext);
        }

        private bool IsValidRequest(HttpContext httpContext, out ContentLoadingContext context, out List<IContentLoader> loaders)
        {
            context = null;
            loaders = null;

            // must be HTTP GET
            if (httpContext.Request.Method != HttpMethods.Get)
            {
                return false;
            }

            if (!httpContext.Request.Path.HasValue)
            {
                return false;
            }

            // check that request path is valid
            var path = httpContext.Request.Path.ToString().ToLower(CultureInfo.InvariantCulture);
            if (!_paths.TryGetValue(path, out IContentInfo info))
            {
                return false;
            }

            _logger.LogDebug($"Started to handle a HTTP request - path is '{httpContext.Request.Path}'.");

            // an optional filter
            var filter = httpContext.Request.Query["filter"].FirstOrDefault();

            // content loaders 
            loaders = _utils.GetContentLoaders(httpContext, info);
            if (!loaders.Any())
            {
                return false;
            }

            // HTTP ETag
            string eTag = null;
            if (_options.Value.SupportETag)
            {
                var eTagFound = TryParseHttpETag(httpContext.Request, out eTag);
                if (eTagFound && loaders.Count > 1)
                {
                    eTag = null;
                    _logger.LogDebug("HTTP ETag will be disabled for this request, because there are multiple content loaders for this request.");
                }
            }

            // resolve a theme that will be used
            var theme = _utils.GetTheme(httpContext);

            _logger.LogDebug($"A theme '{theme.GetName()}' will be used.");

            context = new ContentLoadingContext(httpContext, info, theme, filter, eTag);

            return true;
        }

        private async Task WriteResponseAsync(ContentLoadingContext context, IList<ContentLoaderResponse> responses)
        {
            ContentLoaderResponse response = _utils.BundleResponses(context, responses);

            context.HttpContext.Response.ContentType = context.ContentInfo.MimeType;

            // ok
            if (response.Status == ResponseStatus.Ok)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

                if (!string.IsNullOrEmpty(response.ETag) && _options.Value.SupportETag && responses.Count == 1)
                {
                    UseHttpETag(context.HttpContext.Response, response.ETag);
                }

                await context.HttpContext.Response.Body.WriteAsync(response.Data, 0, response.Data.Length);
            }

            // not modified
            else if (response.Status == ResponseStatus.NotModified)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
            }
        }

        /// <summary>
        /// Tries to parse HTTP ETag from the HTTP request.
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <param name="value">parse HTTP ETag</param>
        /// <returns>true if succeed</returns>
        private static bool TryParseHttpETag(HttpRequest request, out string value)
        {
            value = null;
            if (request.Headers.ContainsKey("If-None-Match"))
            {
                value = request.Headers["If-None-Match"].FirstOrDefault();
            }
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Uses specified HTTP ETag with the HTTP response.
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <param name="value">HTTP ETag</param>
        private static void UseHttpETag(HttpResponse response, string value)
        {
            response.Headers.Add("ETag", new StringValues(value));
        }

    }
}
