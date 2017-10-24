using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Chame.Middlewares
{
    /// <summary>
    /// A middleware for loading JavaScript and CSS content.
    /// </summary>
    internal sealed class ContentLoaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ContentLoaderOptions _options;
        private readonly ILogger<ContentLoaderMiddleware> _logger;

        /// <summary>
        /// All valid query paths and corresponding content-types.
        /// </summary>
        private readonly Dictionary<string, IContentInfo> _validPaths = new Dictionary<string, IContentInfo>();

        public ContentLoaderMiddleware(RequestDelegate next, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger, string pathTemplate)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;

            foreach (IContentInfo ci in _options.ContentModel.SupportedContent)
            {
                string path = string.Format(pathTemplate, ci.Code);
                _validPaths[path] = ci;
                _logger.LogDebug(string.Format("A content loader path '{0}' registered for MIME type '{1}'.", path, ci.MimeType));
            }
        }

        public async Task Invoke(HttpContext httpContext, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger)
        {
            if (TryParse(httpContext, out Tuple<ContentLoadingContext, List<IContentLoader>> assets))
            {
                await HandleAsync(assets.Item1, assets.Item2);
            }
            else
            {
                await _next(httpContext);
            }
        }

        private bool TryParse(HttpContext httpContext, out Tuple<ContentLoadingContext, List<IContentLoader>> assets)
        {
            assets = null;

            // Must be HTTP GET.
            if (httpContext.Request.Method != HttpMethods.Get)
            {
                return false;
            }

            // Check that request path is valid.
            string path = httpContext.Request.Path.ToString();
            IContentInfo ci;
            if (!_validPaths.TryGetValue(path, out ci))
            {
                return false;
            }

            _logger.LogInformation(string.Format("Started to handle the current HTTP request [path = {0}].", httpContext.Request.Path.ToString()));

            // Parse an optional filter.
            string filter = httpContext.Request.Query["filter"].FirstOrDefault();

            // Get content loaders from request services and options.
            List<IContentLoader> loaders = new List<IContentLoader>();
            foreach (IContentLoader loader in httpContext.RequestServices.GetServices<IContentLoader>().Concat(_options.ContentLoaders))
            {
                // TODO: Kuinka tässä, jos content-loader käsittelee kaikki tyypit
                if (loader.SupportedContentTypes().Any(x => x == ci.Code))
                {
                    loaders.Add(loader);
                }                
            }

            if (loaders.Count == 0)
            {
                _logger.LogCritical("No content loaders found.");
                return false;
            }

            // Sort service content loaders if needed.
            if (loaders.Count > 1)
            {
                IContentLoaderSorter sorter = _options.ContentLoaderSorter;
                if (sorter != null)
                {
                    sorter.Sort(loaders);
                }
                else
                {
                    _logger.LogWarning(string.Format("{0} implementation is not configured. Content loaders are invoked in arbitrary order.", nameof(IContentLoaderSorter)));
                }
            }

            // HTTP ETag is not supported if there are multiple content loaders.
            string eTag = null;
            if (_options.SupportETag)
            {
                if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
                {
                    if (loaders.Count == 1)
                    {
                        eTag = httpContext.Request.Headers["If-None-Match"].FirstOrDefault();
                    }
                    else
                    {
                        _logger.LogDebug("Found HTTP ETag will be ignored, because the feature is not available when working with multiple content loaders.");
                    }
                }
            }

            // Resolve theme by invoking ThemeResolver. If not available, a fallback theme comes from options.
            string theme = null;
            IThemeResolver themeResolver = _options.ThemeResolver;
            if (themeResolver != null)
            {
                theme = themeResolver.GetTheme(new ContentFileThemeResolvingContext(httpContext, ci, filter));
            }

            if (string.IsNullOrEmpty(theme))
            {
                theme = _options.DefaultTheme;
                if (string.IsNullOrEmpty(theme))
                {
                    _logger.LogCritical("Could not resolve theme.");
                    return false;
                }
            }

            ContentLoadingContext context = new ContentLoadingContext(httpContext, ci, theme, filter, eTag);

            assets = new Tuple<ContentLoadingContext, List<IContentLoader>>(context, loaders);

            return true;
        }

        private async Task HandleAsync(ContentLoadingContext context, List<IContentLoader> loaders)
        {
            var contents = new List<TextContent>();

            foreach (IContentLoader loader in loaders)
            {
                _logger.LogDebug(string.Format("Loading content by using '{0}' loader.", loader.GetType().FullName));

                // Load content
                TextContent content;
                try
                {
                    content = await loader.LoadContentAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Loader '{0}' threw an exception {1}.", loader.GetType().FullName, ex);
                    throw;
                }

                // Validate loaded content
                if (content == null || content.Status == ResponseStatus.NotFound)
                {
                    _logger.LogDebug(string.Format("Loader '{0}' did not found any content.", loader.GetType().FullName));
                }
                else
                {
                    if (content.Status == ResponseStatus.Ok)
                    {
                        if (content.Value != null && content.Encoding != null)
                        {
                            contents.Add(content);
                        }
                        else
                        {
                            _logger.LogWarning(string.Format("Loader '{0}' retuned null content and/or null encoding, ignoring item.", loader.GetType().FullName));
                        }
                    }
                    else if (content.Status == ResponseStatus.NotModified)
                    {
                        if (_options.SupportETag && !string.IsNullOrEmpty(context.ETag) && loaders.Count == 1)
                        {
                            contents.Add(content);
                        }
                        else
                        {
                            _logger.LogWarning(string.Format("Loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseStatus.NotModified));
                        }
                    }
                }
            }

            if (contents.Count == 0)
            {
                // Not found
                _logger.LogDebug("None of the loader(s) found any content.");
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else if (contents.Count > 0)
            {
                TextContent content = contents.Count > 1 ? TextContent.Combine(contents) : contents.First();

                //if (context.Category == ContentCategory.Js)
                //{
                //    context.HttpContext.Response.ContentType = "application/javascript";
                //}
                //else if (context.Category == ContentCategory.Css)
                //{
                //    context.HttpContext.Response.ContentType = "text/css";
                //}
                context.HttpContext.Response.ContentType = context.ContentInfo.MimeType;

                if (content.Status == ResponseStatus.Ok)
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    if (_options.SupportETag && contents.Count == 1 && !string.IsNullOrEmpty(content.ETag))
                    {
                        context.HttpContext.Response.Headers.Add("ETag", new StringValues(content.ETag));
                    }
                    await context.HttpContext.Response.WriteAsync(content.Value, content.Encoding);
                }
                else if (content.Status == ResponseStatus.NotModified)
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
            }
        }

    }
}
