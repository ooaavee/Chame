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

        public ContentLoaderMiddleware(RequestDelegate next, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger)
        {
            if (TryParse(httpContext, out ContentLoadingContext context))
            {
                await HandleAsync(context);
            }
            else
            {
                await _next(httpContext);
            }
        }

        private bool TryParse(HttpContext httpContext, out ContentLoadingContext context)
        {
            context = null;

            // Must be HTTP GET
            if (httpContext.Request.Method != HttpMethods.Get)
            {
                return false;
            }

            // Is this JavaScript request?
            if (string.IsNullOrEmpty(_options.VirtualPathForJsRequests))
            {
                _logger.LogCritical("VirtualPathForJsRequests is missing.");
                return false;
            }
            bool isJs = httpContext.Request.Path.StartsWithSegments(new PathString(_options.VirtualPathForJsRequests));

            // Is this CSS request?
            if (string.IsNullOrEmpty(_options.VirtualPathForCssRequests))
            {
                _logger.LogCritical("VirtualPathForCssRequests is missing.");
                return false;
            }
            bool isCss = httpContext.Request.Path.StartsWithSegments(new PathString(_options.VirtualPathForCssRequests));

            // So, no JavaScript or CSS -> just continue your life...
            if (!(isJs || isCss))
            {
                return false;
            }

            _logger.LogInformation(string.Format("Started to handle the current HTTP request [path = {0}].", httpContext.Request.Path.ToString()));

            // Filter
            string filter = httpContext.Request.Query["filter"].FirstOrDefault();

            // Content loaders
            IContentLoader func = null;
            List<IContentLoader> contentLoaders = new List<IContentLoader>();

            // Category
            ContentCategory category = default(ContentCategory);

            if (isJs)
            {
                category = ContentCategory.Js;
                if (_options.JsLoader != null)
                {
                    func = new FuncContentLoader(_options.JsLoader, int.MinValue);
                }
                contentLoaders.AddRange(httpContext.RequestServices.GetServices<IJsContentLoader>());
            }

            if (isCss)
            {
                category = ContentCategory.Css;
                if (_options.CssLoader != null)
                {
                    func = new FuncContentLoader(_options.CssLoader, int.MinValue);
                }
                contentLoaders.AddRange(httpContext.RequestServices.GetServices<ICssContentLoader>());
            }

            int serviceContentLoaderCount = contentLoaders.Count;
            int totalContentLoaderCount = contentLoaders.Count + (func != null ? 1 : 0);

            if (totalContentLoaderCount == 0)
            {
                _logger.LogCritical("No content loaders found.");
                return false;
            }

            // Sort service content loaders if needed.
            if (serviceContentLoaderCount > 1)
            {
                if (_options.ContentLoaderSorter != null)
                {
                    _options.ContentLoaderSorter(contentLoaders);
                }
                else
                {
                    _logger.LogWarning("There is no content loader sorter available, which means that content loaders are invoked in arbitrary order!");
                }
            }

            // This one is always the first content loader.
            if (func != null)
            {
                contentLoaders.Insert(0, func);
            }

            // HTTP ETag is not supported if there are multiple content loaders.
            string eTag = null;
            if (_options.SupportETag)
            {
                if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
                {
                    if (totalContentLoaderCount == 1)
                    {
                        eTag = httpContext.Request.Headers["If-None-Match"].FirstOrDefault();
                    }
                    else
                    {
                        _logger.LogDebug("HTTP ETag found from request headers, but won't be used, because there are multiple content loaders.");
                    }
                }
            }

            // Resolve theme by invoking ThemeResolver. If not available, a fallback theme comes from options.
            string theme = null;
            if (_options.ThemeResolver != null)
            {
                theme = _options.ThemeResolver.GetTheme(new ContentFileThemeResolvingContext(httpContext, category, filter));
            }

            if (string.IsNullOrEmpty(theme))
            {
                theme = _options.DefaultTheme;
                if (string.IsNullOrEmpty(theme))
                {
                    _logger.LogCritical("DefaultTheme is missing.");
                    return false;
                }
            }

            context = new ContentLoadingContext(httpContext, category, theme, filter, eTag, contentLoaders);

            return true;
        }

        private async Task HandleAsync(ContentLoadingContext context)
        {
            var contents = new List<TextContent>();

            foreach (IContentLoader loader in context.ContentLoaders)
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
                        if (_options.SupportETag && !string.IsNullOrEmpty(context.ETag) && context.ContentLoaders.Count == 1)
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

                if (context.Category == ContentCategory.Js)
                {
                    context.HttpContext.Response.ContentType = "application/javascript";
                }
                else if (context.Category == ContentCategory.Css)
                {
                    context.HttpContext.Response.ContentType = "text/css";
                }

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
