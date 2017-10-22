using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace Chame
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private const string ThemeKey = "chame.theme";

        private readonly RazorThemeOptions _options;

        public ThemedViewLocationExpander(RazorThemeOptions options)
        {
            _options = options;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Check that HTTP Context is available!
            if (context.ActionContext.HttpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            IOptions<ContentLoaderOptions> options = context.ActionContext.HttpContext.RequestServices.GetService(typeof(IOptions<ContentLoaderOptions>)) as IOptions<ContentLoaderOptions>;          
            if (options?.Value == null)
            {
                throw new InvalidOperationException(string.Format("{0} is not available.", nameof(IOptions<ContentLoaderOptions>)));
            }

            // Resolve theme by invoking ThemeResolver. If not available, a fallback theme comes from options.   
            string theme = null;
            if (options.Value.ThemeResolver != null)
            {
                theme = options.Value.ThemeResolver.GetTheme(new RazorThemeResolvingContext(
                    context.ActionContext.HttpContext,
                    context.ViewName,
                    context.ControllerName,
                    context.PageName,
                    context.AreaName,
                    context.IsMainPage,
                    context.Values));
            }

            if (string.IsNullOrEmpty(theme))
            {
                theme = options.Value.DefaultTheme;
                if (string.IsNullOrEmpty(theme))
                {
                    throw new InvalidOperationException("Theme not found.");
                }
            }

            context.Values[ThemeKey] = theme;
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
