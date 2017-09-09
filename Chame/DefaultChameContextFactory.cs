using System;
using System.Linq;
using Chame.Loaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chame
{
    public class DefaultChameContextFactory : IChameContextFactory
    {
        private readonly ChameOptions _options;
        private readonly ILogger<DefaultChameContextFactory> _logger;

        public DefaultChameContextFactory(ChameOptions options, ILogger<DefaultChameContextFactory> logger)
        {
            _options = options;
            _logger = logger;
        }

        public bool TryCreateContext(HttpContext httpContext, out ChameContext context)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
         
            _logger.LogDebug("started to handle the current HTTP request");

            context = null;

            bool valid = false;

            if (httpContext.Request.Method == "GET" && httpContext.Request.Path.HasValue)
            {
                string path = httpContext.Request.Path.Value.ToLower();

                if (path.StartsWith("/chame/js") || path.StartsWith("/chame/css"))
                {
                    valid = true;

                    _logger.LogInformation(string.Format("started to handle the current HTTP request (path = {0})", path));

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
                        _logger.LogCritical("no content loaders found");
                        valid = false;
                    }
                    else
                    {
                        _logger.LogInformation(string.Format("found {0} content loader(s)", loaderCount));

                        // Sort content loaders if required
                        if (loaderCount > 1)
                        {
                            _options.SortContentLoaders(loaders);
                        }

                        // HTTP ETag is not supported if there are multiple content loader!
                        string eTag = null;
                        if (_options.ETagEnabled)
                        {
                            if (loaderCount == 1)
                            {
                                if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
                                {
                                    eTag = httpContext.Request.Headers["If-None-Match"].First();

                                    _logger.LogInformation(string.Format("using HTTP ETag {0}", eTag));
                                }
                            }
                        }

                        // Finally create the context object
                        context = new ChameContext(httpContext, category, filter, eTag, loaders);

                        _logger.LogDebug("ChameContext created for the current HTTP request");
                    }
                }
                else
                {
                    _logger.LogDebug("ignoring the current HTTP request: request method is not GET and path doesn't start with /chame/js or /chame/css");
                }
            }

            return valid;
        }
    }
}
