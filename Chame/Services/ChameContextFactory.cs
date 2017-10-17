using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.Services
{
    /// <summary>
    /// This is a factory class for building <see cref="ChameContext"/> objects.
    /// </summary>
    internal sealed class ChameContextFactory
    {
        private const string PathForJs = "/chame-js-loader";
        private const string PathForCss = "/chame-css-loader";

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
                theme = _options.ThemeResolver.GetTheme(new ChameContentFileThemeResolveContext(httpContext, category, filter));
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

            
            context = ChameContext.Create(httpContext, category, theme, filter, eTag, loaders);
            _logger.LogDebug("ChameContext created for the current HTTP request.");
            return true;
        }


        private sealed class FuncLoader : IContentLoader
        {
            private readonly Func<ChameContext, Task<ResponseContent>> _func;

            public FuncLoader(Func<ChameContext, Task<ResponseContent>> func)
            {
                _func = func;
            }

            public int Priority => int.MinValue;

            public async Task<ResponseContent> LoadAsync(ChameContext context)
            {
                return await _func(context);
            }
        }

    }
}
