using System;
using System.Linq;
using Chame.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.Services
{
    internal sealed class ContextFactory : IChameContextFactory
    {
        private readonly ChameOptions _options;
        private readonly ILogger<ContextFactory> _logger;

        public ContextFactory(IOptions<ChameOptions> options, ILogger<ContextFactory> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool TryCreateContext(HttpContext httpContext, out ChameContext context)
        {
            _logger.LogDebug("Started to handle the current HTTP request.");

            context = null;

            bool valid = false;

            if (httpContext.Request.Method == "GET" && httpContext.Request.Path.HasValue)
            {
                string path = httpContext.Request.Path.Value.ToLower();

                if (path.StartsWith("/chame/js") || path.StartsWith("/chame/css"))
                {
                    valid = true;

                    _logger.LogInformation(string.Format("Started to handle the current HTTP request (path = {0}).", path));

                    string[] tokens = path.Split('/');

                    // Parse catagory
                    ContentCategory category = default(ContentCategory);
                    if (tokens.Length == 3 || tokens.Length == 4)
                    {
                        switch (tokens[2])
                        {
                            case "js":
                                category = ContentCategory.Js;
                                break;
                            case "css":
                                category = ContentCategory.Css;
                                break;
                        }
                    }

                    // Parse filter (optional)
                    string filter = null;
                    if (tokens.Length == 4 && !string.IsNullOrEmpty(tokens[3]))
                    {
                        filter = tokens[3];
                    }

                    // Resolve content loaders
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
                            Action<IContentLoader[]> sorter = _options.Events.ContentLoaderSorter;
                            if (sorter != null)
                            {
                                sorter(loaders);
                            }
                            else
                            {
                                _logger.LogWarning("No content loader sorter available, using content loaders in arbitrary order.");
                            }
                        }

                        // HTTP ETag is not supported if there are multiple content loader!
                        string eTag = null;
                        if (_options.UseETag)
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
                        Func<ThemeResolverEventArgs, string> resolver = _options.Events.ThemeResolver;
                        if (resolver != null)
                        {
                            theme = resolver(new ThemeResolverEventArgs
                            {
                                HttpContext = httpContext,
                                Category = category,
                                Filter = filter
                            });

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
                    _logger.LogDebug("Ignoring the current HTTP request, because request method is not GET and path doesn't start with '/chame/js' or '/chame/css'.");
                }
            }

            return valid;
        }
    }
}
