using System;
using System.Collections.Generic;
using System.Linq;
using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private const string ThemeKey = "chame.theme.id";

        private readonly RazorThemeOptions _options;

        public ThemedViewLocationExpander(RazorThemeOptions options)
        {
            _options = options;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            HttpContext httpContext = context.ActionContext.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            ILogger<ThemedViewLocationExpander> logger = httpContext.RequestServices.GetRequiredService<ILogger<ThemedViewLocationExpander>>();

            IOptions<ContentLoaderOptions> options = httpContext.RequestServices.GetRequiredService<IOptions<ContentLoaderOptions>>();
           
            // resolve theme
            IThemeInfo theme = ThemeResolver.Resolve(new RazorThemeResolvingContext(context), options.Value.ThemeResolver, options.Value.DefaultTheme);
            if (theme == null)
            {
                logger.LogCritical("Could not resolve a theme.");
                throw new InvalidOperationException("Could not resolve a theme.");
            }

            context.Values[ThemeKey] = theme.Id;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(ThemeKey, out string theme))
            {
                IEnumerable<string> themeLocations = GetViewLocationForTheme(theme);
                viewLocations = themeLocations.Concat(viewLocations);
            }
            return viewLocations;
        }

        private IEnumerable<string> GetViewLocationForTheme(string theme)
        {
            foreach (string template in _options.ViewLocationTemplates)
            {
                string location = string.Format(template, theme);
                yield return location;
            }
        }

    }
}
