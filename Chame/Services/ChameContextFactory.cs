using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Chame.Services
{
    /// <summary>
    /// This is a factory class for building ChameContext objects.
    /// </summary>
    internal sealed class ChameContextFactory
    {
        private readonly ChameOptions _options;
        private readonly ILogger<ChameContextFactory> _logger;

        public ChameContextFactory(IOptions<ChameOptions> options, ILogger<ChameContextFactory> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool TryCreate(HttpContext httpContext, out ChameContext context)
        {
            _logger.LogDebug("Started to handle the current HTTP request.");

            context = null;

            bool valid = false;

            if (httpContext.Request.Method == HttpMethods.Get)
            {
                bool js = httpContext.Request.Path.StartsWithSegments(new PathString("/chame-js-loader"));
                bool css = httpContext.Request.Path.StartsWithSegments(new PathString("/chame-css-loader"));

                if (js || css)
                {
                    valid = true;

                    _logger.LogInformation(string.Format("Started to handle the current HTTP request (path = {0}).", httpContext.Request.Path.ToString()));

                    // Category
                    ContentCategory category = js ? ContentCategory.Js : ContentCategory.Css;

                    // Filter
                    StringValues parameter = httpContext.Request.Query["filter"];
                    string filter = parameter.FirstOrDefault();

                    // Content loaders
                    IContentLoader[] loaders = null;
                    switch (category)
                    {
                        case ContentCategory.Js:
                            loaders = httpContext.RequestServices.GetServices<IJsLoader>().ToArray();
                            break;
                        case ContentCategory.Css:
                            loaders = httpContext.RequestServices.GetServices<ICssLoader>().ToArray();
                            break;
                    }

                    int loaderCount = loaders?.Length ?? 0;

                    if (loaderCount == 0)
                    {
                        _logger.LogCritical("No content loaders found.");
                        valid = false;
                    }
                    else
                    {
                        _logger.LogInformation(string.Format("Found {0} content loader(s).", loaderCount));

                        // Sort content loaders if required
                        if (loaderCount > 1)
                        {
                            if (_options.ContentLoaderSorter != null)
                            {
                                _options.ContentLoaderSorter(loaders);
                            }
                            else
                            {
                                _logger.LogWarning("No content loader sorter available, using content loaders in arbitrary order.");
                            }
                        }

                        // HTTP ETag is not supported if there are multiple content loader!
                        string eTag = null;
                        if (_options.SupportETag)
                        {
                            if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
                            {
                                if (loaderCount == 1)
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

                        // Resolve theme
                        string theme = null;
                        if (_options.ThemeResolver != null)
                        {
                            theme = _options.ThemeResolver(new ThemeResolverEventArgs(httpContext, category, filter));
                            _logger.LogDebug(theme == null ? "Theme resolver returned a null value, the default theme will be used." : string.Format("Theme resolver retuned theme '{0}', we'll use that.", theme));
                        }

                        if (theme == null)
                        {
                            theme = _options.DefaultTheme ?? ChameOptions.DefaultThemeName;
                        }

                        // Finally create the context object
                        context = ChameContext.Create(httpContext, category, filter, eTag, theme, loaders);

                        _logger.LogDebug("ChameContext created for the current HTTP request.");
                    }
                }
                else
                {
                    _logger.LogDebug("Ignoring the current HTTP request, because request method is not GET and path is not '/chame-js-loader' or '/chame-css-loader'.");
                }
            }

            return valid;
        }
    }
}
