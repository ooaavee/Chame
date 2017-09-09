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

            // TODO: Logging

            context = null;

            bool valid = false;

            if (httpContext.Request.Method == "GET" && httpContext.Request.Path.HasValue)
            {
                string path = httpContext.Request.Path.Value.ToLower();

                if (path.StartsWith("/chame/"))
                {
                    string[] tokens = path.Split('/');

                    // Parse catagory
                    ContentCategory category = default(ContentCategory);
                    if (tokens.Length == 3 || tokens.Length == 4)
                    {
                        switch (tokens[2])
                        {
                            case "js":
                                category = ContentCategory.Js;
                                valid = true;
                                break;
                            case "css":
                                category = ContentCategory.Css;
                                valid = true;
                                break;
                        }
                    }

                    if (valid)
                    {
                        // Parse filter (optional)
                        string filter = null;
                        if (tokens.Length == 4 && !string.IsNullOrEmpty(tokens[3]))
                        {
                            filter = tokens[3];
                        }

                        IContentLoader[] loaders;
                        switch (category)
                        {
                            case ContentCategory.Js:
                                loaders = httpContext.RequestServices.GetServices<IJsLoader>().ToArray();
                                break;
                            case ContentCategory.Css:
                                loaders = httpContext.RequestServices.GetServices<ICssLoader>().ToArray();
                                break;
                            default:
                                throw new InvalidOperationException("fuck");
                        }

                        int loaderCount = loaders.Length;

                        if (loaderCount == 0)
                        {
                            // TODO: Kirjoita lokille, että ei löydy yhtään loaderia!
                            valid = false;
                        }
                        else
                        {
                            // Sort content loaders if required
                            if (loaderCount > 1)
                            {
                                loaders = _options.SortContentLoaders(loaders);
                            }

                            // HTTP ETag is not supported if there are multiple content loader!
                            string eTag = null;
                            if (loaderCount == 1)
                            {
                                if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
                                {
                                    eTag = httpContext.Request.Headers["If-None-Match"].First();
                                }
                            }

                            // Finally create the context object
                            context = new ChameContext(httpContext, category, filter, eTag, loaders);
                        }

                    }
                }
            }

            return valid;
        }
    }
}
