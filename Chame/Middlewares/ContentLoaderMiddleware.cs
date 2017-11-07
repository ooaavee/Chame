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
        private readonly IThemeResolver _themeResolver;

        private readonly ILogger<ContentLoaderMiddleware> _logger;

        /// <summary>
        /// All valid request paths and corresponding content-types.
        /// </summary>
        private readonly IDictionary<string, IContentInfo> _pathMap = new Dictionary<string, IContentInfo>();

        public ContentLoaderMiddleware(RequestDelegate next, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger)
        {
            _next = next;

            ContentLoaderOptions opt = options.Value;

            _supportETag = opt.SupportETag;
            _contentLoaders = opt.ContentLoaders;
            _contentLoaderSorter = opt.ContentLoaderSorter;
            _defaultTheme = opt.DefaultTheme;
            _themeResolver = opt.ThemeResolver;

            // resolves valid requests paths for this middleware
            var template = opt.RequestPathTemplate;
            foreach (IContentInfo content in opt.ContentModel.SupportedContent)
            {
                var path = string.Format(template, content.Extension).ToLower(CultureInfo.InvariantCulture);
                _pathMap.Add(path, content);
                //logger.LogDebug($"Registering path '{path}' for MIME type {content.MimeType}.");
            }

            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (TryGetContentInfo(httpContext, out IContentInfo contentInfo))
            {
                if (TryGetAssets(httpContext, contentInfo, out ContentLoadingAssets assets))
                {
                    var responses = await LoadContentAsync(assets);
                    if (responses.Any())
                    {
                        await WriteResponseAsync(assets.Context, responses);
                        return;
                    }
                }
            }

            await _next(httpContext);
        }

        private bool TryGetContentInfo(HttpContext httpContext, out IContentInfo contentInfo)
        {
            contentInfo = null;

            // must be HTTP GET
            if (httpContext.Request.Method != HttpMethods.Get)
            {
                return false;
            }

            // check that request path is valid
            string path = httpContext.Request.Path.ToString().ToLower(CultureInfo.InvariantCulture);
            if (!_pathMap.TryGetValue(path, out contentInfo))
            {
                return false;
            }

            return true;
        }

        private bool TryGetAssets(HttpContext httpContext, IContentInfo contentInfo, out ContentLoadingAssets assets)
        {
            assets = null;

            _logger.LogInformation(string.Format("Started to handle a HTTP request [path = {0}].", httpContext.Request.Path.ToString()));

            // parse an optional filter
            string filter = httpContext.Request.Query["filter"].FirstOrDefault();

            // get content loaders from request services and options
            List<IContentLoader> loaders = new List<IContentLoader>();
            foreach (IContentLoader loader in httpContext.RequestServices.GetServices<IContentLoader>().Concat(_contentLoaders))
            {
                if (loader.Supports().Any(supports => supports == contentInfo.Extension || supports == ContentLoaderOptions.ContentLoaderSupportsAll))
                {
                    loaders.Add(loader);
                }
                else
                {
                    _logger.LogDebug($"Ignoring content loader '{loader.GetType().FullName}' - it doesn't suppport '{contentInfo.Extension}'.");
                }
            }

            int loaderCount = loaders.Count;

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
                bool eTagFound = HttpETagHelper.TryParse(httpContext.Request, out eTag);
                if (eTagFound && loaderCount > 1)
                {
                    eTag = null;
                    _logger.LogDebug("HTTP ETag will be disabled for this request, because there are multiple content loaders for this request.");
                }
            }

            // resolve theme
            ITheme theme = ThemeHelper.ResolveTheme(new ContentFileThemeResolvingContext(httpContext, contentInfo, filter), _themeResolver, _defaultTheme);
            if (theme == null)
            {
                _logger.LogCritical("Could not resolve theme.");
                return false;
            }

            _logger.LogInformation(string.Format("A theme '{0}' will be used.", theme));

            var context = new ContentLoadingContext(httpContext, contentInfo, theme, filter, eTag);

            assets = new ContentLoadingAssets {Context = context, ContentLoaders = loaders};

            return true;
        }

        private async Task<IList<ContentLoaderResponse>> LoadContentAsync(ContentLoadingAssets assets)
        {
            var responses = new List<ContentLoaderResponse>();

            var eTag = assets.Context.ETag;

            var loaderCount = assets.ContentLoaders.Count;

            foreach (IContentLoader loader in assets.ContentLoaders)
            {
                _logger.LogDebug(string.Format("Loading content by using '{0}' content loader.", loader.GetType().FullName));

                // load stuff
                ContentLoaderResponse response;
                try
                {
                    response = await loader.LoadContentAsync(assets.Context);
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

        private class ContentLoadingAssets
        {
            public ContentLoadingContext Context { get; set; }
            public IList<IContentLoader> ContentLoaders { get; set; }
        }

    }
}
