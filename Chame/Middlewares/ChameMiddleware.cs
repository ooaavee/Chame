using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Chame.Middlewares
{
    /// <summary>
    /// A middleware for handling Chame web-requests.
    /// </summary>
    internal sealed class ChameMiddleware
    {
        private readonly RequestDelegate _next;

        public ChameMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IOptions<ChameOptions> options, ILogger<ChameMiddleware> logger)
        {
            var parser = new RequestParser(httpContext, options, logger);

            if (parser.TryParse(httpContext, out ContentLoadingContext context))
            {
                var handler = new RequestHandler(options, logger);

                await handler.HandleAsync(context);
            }
            else
            {
                await _next(httpContext);
            }
        }

        private sealed class RequestParser
        {
            private const string PathForJs = "/chame-js-loader";
            private const string PathForCss = "/chame-css-loader";

            private readonly ChameOptions _options;
            private readonly ILogger<ChameMiddleware> _logger;

            public RequestParser(HttpContext httpContext, IOptions<ChameOptions> options, ILogger<ChameMiddleware> logger)
            {
                _options = options.Value;
                _logger = logger;
            }

            public bool TryParse(HttpContext httpContext, out ContentLoadingContext context)
            {
                context = null;
                _logger.LogDebug("Started to handle the current HTTP request.");

                // Must be HTTP GET
                if (httpContext.Request.Method != HttpMethods.Get)
                {
                    return false;
                }

                // Validate request path
                bool isJs = httpContext.Request.Path.StartsWithSegments(new PathString(PathForJs));
                bool isCss = httpContext.Request.Path.StartsWithSegments(new PathString(PathForCss));

                if (!(isJs || isCss))
                {
                    return false;
                }

                _logger.LogInformation(string.Format("Started to handle the current HTTP request [path = {0}].", httpContext.Request.Path.ToString()));

                // Category
                ContentCategory category = isJs ? ContentCategory.Js : ContentCategory.Css;

                // Filter
                string filter = httpContext.Request.Query["filter"].FirstOrDefault();

                // Content loaders
                List<IContentLoader> loaders = new List<IContentLoader>();

                switch (category)
                {
                    case ContentCategory.Js:
                        if (_options.JsLoader != null)
                        {
                            loaders.Add(new FuncLoader(_options.JsLoader));
                        }
                        loaders.AddRange(httpContext.RequestServices.GetServices<IJsContentLoader>());
                        break;

                    case ContentCategory.Css:
                        if (_options.CssLoader != null)
                        {
                            loaders.Add(new FuncLoader(_options.CssLoader));
                        }
                        loaders.AddRange(httpContext.RequestServices.GetServices<ICssContentLoader>());
                        break;
                }

                if (!loaders.Any())
                {
                    _logger.LogCritical("No content loaders found.");
                    return false;
                }

                if (loaders.Count > 1)
                {
                    if (_options.ContentLoaderSorter != null)
                    {
                        _options.ContentLoaderSorter(loaders);
                    }
                    else
                    {
                        _logger.LogWarning("There is no content loader sorter available, which means that content loaders are invoked in arbitrary order!");
                    }
                }

                // HTTP ETag is not supported if there are multiple content loader.
                string eTag = null;
                if (_options.SupportETag)
                {
                    if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
                    {
                        if (loaders.Count == 1)
                        {
                            eTag = httpContext.Request.Headers["If-None-Match"].First();
                            _logger.LogDebug(string.Format("HTTP ETag {0} found from request headers.", eTag));
                        }
                        else
                        {
                            _logger.LogDebug("HTTP ETag {0} found from request headers, but won't be used, because there are multiple content loaders.");
                        }
                    }
                }

                // Resolve theme by invoking ThemeResolver. If not available, a fallback theme comes from ChameOptions.
                string theme = null;
                if (_options.ThemeResolver != null)
                {
                    theme = _options.ThemeResolver.ResolveTheme(new ContentFileThemeResolveContext(httpContext, category, filter));
                }

                if (string.IsNullOrEmpty(theme))
                {
                    if (!string.IsNullOrEmpty(_options.DefaultTheme))
                    {
                        theme = _options.DefaultTheme;
                    }
                    else
                    {
                        _logger.LogCritical("DefaultTheme is missing.");
                    }
                    if (string.IsNullOrEmpty(theme))
                    {
                        return false;
                    }
                }

                context = new ContentLoadingContext
                {
                    HttpContext = httpContext,
                    Category = category,
                    Theme = theme,
                    Filter = filter,
                    ETag = eTag,
                    ContentLoaders = loaders
                };

                _logger.LogDebug("ChameContext created for the current HTTP request.");

                return true;
            }
        }

        private sealed class RequestHandler
        {
            private readonly ChameOptions _options;
            private readonly ILogger<ChameMiddleware> _logger;

            public RequestHandler(IOptions<ChameOptions> options, ILogger<ChameMiddleware> logger)
            {
                _options = options.Value;
                _logger = logger;
            }

            public async Task HandleAsync(ContentLoadingContext context)
            {
                var responses = new List<ResponseContent>();

                foreach (IContentLoader loader in context.ContentLoaders)
                {
                    _logger.LogDebug(string.Format("Loading content by using '{0}' loader.", loader.GetType().FullName));

                    // Load content
                    ResponseContent response;
                    try
                    {
                        response = await loader.LoadContentAsync(context);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Loader '{0}' threw an exception {1}.", loader.GetType().FullName, ex);
                        throw;
                    }

                    if (response == null || response.Status == ResponseContentStatus.NotFound)
                    {
                        _logger.LogDebug(string.Format("Loader '{0}' did not found any content.", loader.GetType().FullName));
                    }
                    else switch (response.Status)
                    {
                        case ResponseContentStatus.OK:
                            if (response.Content != null && response.Encoding != null)
                            {
                                responses.Add(response);
                            }
                            else
                            {
                                _logger.LogWarning(string.Format("Loader '{0}' retuned null content and/or null encoding, ignoring item.", loader.GetType().FullName));
                            }
                            break;

                        case ResponseContentStatus.NotModified:
                            if (_options.SupportETag && !string.IsNullOrEmpty(context.ETag) && context.ContentLoaders.Count == 1)
                            {
                                responses.Add(response);
                            }
                            else
                            {
                                _logger.LogWarning(string.Format("Loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseContentStatus.NotModified));
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                int reponseCount = responses.Count;

                if (reponseCount == 0)
                {
                    _logger.LogDebug("None of loader(s) found any content.");

                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                else if (reponseCount > 0)
                {
                    ResponseContent response = reponseCount > 1 ? Merge(responses) : responses.First();

                    switch (context.Category)
                    {
                        case ContentCategory.Js:
                            context.HttpContext.Response.ContentType = "application/javascript";
                            break;

                        case ContentCategory.Css:
                            context.HttpContext.Response.ContentType = "text/css";
                            break;

                        default:
                            _logger.LogError(string.Format("Invalid content category '{0}'.", context.Category));
                            throw new InvalidOperationException(string.Format("Invalid content category '{0}'.", context.Category));
                    }

                    switch (response.Status)
                    {
                        case ResponseContentStatus.OK:
                            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

                            if (_options.SupportETag && responses.Count == 1 && !string.IsNullOrEmpty(response.ETag))
                            {
                                context.HttpContext.Response.Headers.Add("ETag", new StringValues(response.ETag));
                            }

                            await context.HttpContext.Response.WriteAsync(response.Content, response.Encoding);
                            break;

                        case ResponseContentStatus.NotModified:
                            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                            break;

                        case ResponseContentStatus.NotFound:
                            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            break;

                        default:
                            _logger.LogError(string.Format("Invalid reponse status '{0}'.", response.Status));
                            throw new InvalidOperationException(string.Format("Invalid reponse status '{0}'.", response.Status));
                    }
                }
            }

            /// <summary>
            /// Merges multiple ResponseContent objects.
            /// </summary>
            private ResponseContent Merge(IEnumerable<ResponseContent> items)
            {
                _logger.LogDebug("Merging content from multiple loaders.");

                var response = new ResponseContent { Encoding = null, ETag = null, Status = ResponseContentStatus.OK };
                var content = new StringBuilder();

                foreach (ResponseContent item in items)
                {
                    if (response.Encoding != null && response.Encoding.EncodingName != item.Encoding.EncodingName)
                    {
                        var message = string.Format("Failed to merge ResponseContent object because multiple encondings were used ({0} and {1}).", response.Encoding.EncodingName, item.Encoding.EncodingName);
                        _logger.LogError(message);
                        throw new InvalidOperationException(message);
                    }
                    content.Append(item.Content);
                    response.Encoding = item.Encoding;
                }

                response.Content = content.ToString();
                return response;
            }
        }

        private sealed class FuncLoader : IContentLoader
        {
            private readonly Func<ContentLoadingContext, Task<ResponseContent>> _func;

            public FuncLoader(Func<ContentLoadingContext, Task<ResponseContent>> func)
            {
                _func = func;
            }

            public int Priority => int.MinValue;

            public async Task<ResponseContent> LoadContentAsync(ContentLoadingContext context)
            {
                return await _func(context);
            }
        }

    }
}
