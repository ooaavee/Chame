using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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

        private readonly bool _supportETag;
        private readonly IList<IContentLoader> _contentLoaders;
        private readonly IContentLoaderSorter _contentLoaderSorter;
        private readonly ITheme _defaultTheme;
        private readonly ILogger<ContentLoaderMiddleware> _logger;

        /// <summary>
        /// All valid request paths and corresponding content-types.
        /// </summary>
        private readonly IDictionary<string, IContentInfo> _paths;

        public ContentLoaderMiddleware(RequestDelegate next, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger)
        {
            _next = next;
            _supportETag = options.Value.SupportETag;
            _contentLoaders = options.Value.ContentLoaders;
            _contentLoaderSorter = options.Value.ContentLoaderSorter;
            _defaultTheme = options.Value.DefaultTheme;
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
                var responses = await LoadContentAsync(context, loaders);

                await OnAfterLoadContentAsync(context, responses);

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

            // parse an optional filter
            var filter = http.Request.Query["filter"].FirstOrDefault();

            // get content loaders from request services and options
            loaders = new List<IContentLoader>();
            foreach (var loader in http.RequestServices.GetServices<IContentLoader>().Concat(_contentLoaders))
            {
                if (loader.Supports().Any(supports => supports == info.Extension || supports == ContentLoaderOptions.ContentLoaderSupportsAll))
                {
                    loaders.Add(loader);
                }
                else
                {
                    _logger.LogDebug($"Ignoring content loader '{loader.GetType().FullName}' - it doesn't suppport '{info.Extension}'.");
                }
            }

            var loaderCount = loaders.Count;

            if (loaderCount == 0)
            {
                _logger.LogCritical("No content loaders found.");
                return false;
            }

            // sort content loaders
            if (loaderCount > 1)
            {
                if (_contentLoaderSorter != null)
                {
                    _contentLoaderSorter.Sort(loaders);
                }
                else
                {
                    _logger.LogWarning(string.Format("{0} implementation is not configured. Content loaders are invoked in arbitrary order.", nameof(IContentLoaderSorter)));
                }
            }

            // HTTP ETag
            string eTag = null;
            if (_supportETag)
            {
                var eTagFound = HttpETagHelper.TryParse(http.Request, out eTag);
                if (eTagFound && loaderCount > 1)
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

        private async Task<IList<ContentLoaderResponse>> LoadContentAsync(ContentLoadingContext context, List<IContentLoader> loaders)
        {
            var responses = new List<ContentLoaderResponse>();

            var eTag = context.ETag;

            var loaderCount = loaders.Count;

            foreach (IContentLoader loader in loaders)
            {
                _logger.LogDebug(string.Format("Loading content by using '{0}' content loader.", loader.GetType().FullName));

                // load stuff
                ContentLoaderResponse response;
                try
                {
                    response = await loader.LoadContentAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Content loader '{0}' threw an unhandled exception.", loader.GetType().FullName);
                    throw;
                }

                // not found
                if (response == null || response.Status == ResponseStatus.NotFound)
                {
                    _logger.LogDebug(string.Format("Content loader '{0}' did not found any content.", loader.GetType().FullName));
                }

                // ok
                else if (response.Status == ResponseStatus.Ok)
                {
                    if (response.Data != null)
                    {
                        responses.Add(response);
                    }
                    else
                    {
                        _logger.LogWarning(string.Format("Content loader '{0}' retuned null content -> ignoring item.", loader.GetType().FullName));
                    }
                }

                // not modified
                else if (response.Status == ResponseStatus.NotModified)
                {
                    if (_supportETag && !string.IsNullOrEmpty(eTag) && loaderCount == 1)
                    {
                        responses.Add(response);
                    }
                    else
                    {
                        _logger.LogWarning(string.Format("Content loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseStatus.NotModified));
                    }
                }
            }

            return responses;
        }

        private async Task OnAfterLoadContentAsync(ContentLoadingContext context, IList<ContentLoaderResponse> responses)
        {
            if (!responses.Any())
            {
                var callback = context.HttpContext.RequestServices.GetService(typeof(IContentNotFoundCallback)) as IContentNotFoundCallback;

                if (callback != null)
                {
                    _logger.LogDebug($"Content not found - trying to get default content by using the {nameof(IContentNotFoundCallback)} service.");

                    var data = await callback.GetDefaultContentAsync(context);

                    if (data == null)
                    {
                        _logger.LogDebug($"The {nameof(IContentNotFoundCallback)} service did not return the default content.");
                    }
                    else
                    {
                        _logger.LogDebug($"The {nameof(IContentNotFoundCallback)} service returned the default content.");

                        var file = new FileContent { Data = data };
                        var response = ContentLoaderResponse.Ok(file);
                        responses.Add(response);
                    }
                }
            }
        }

        private async Task WriteResponseAsync(ContentLoadingContext context, IList<ContentLoaderResponse> responses)
        {
            ContentLoaderResponse response;
            if (responses.Count == 1)
            {
                response = responses.First();
            }
            else
            {
                if (context.ContentInfo.AllowBundling)
                {
                    response = ContentLoaderResponse.Bundle(responses);
                }
                else
                {
                    _logger.LogCritical($"Received multiple responses, but '{context.ContentInfo.MimeType}' content cannot be bundled. The first response will be used and others are ignored!");
                    response = responses.First();
                }
            }

            context.HttpContext.Response.ContentType = context.ContentInfo.MimeType;

            // ok
            if (response.Status == ResponseStatus.Ok)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

                if (!string.IsNullOrEmpty(response.ETag) && _supportETag && responses.Count == 1)
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
            return theme ?? _defaultTheme;
        }

    }
}
