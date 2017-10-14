using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace Chame
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ChameViewLocationExpander : IViewLocationExpander
    {
        private const string ThemeKey = "chame.razor.theme";

        private readonly ChameRazorViewEngineOptions _options;

        public ChameViewLocationExpander(ChameRazorViewEngineOptions options)
        {
            _options = options;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Check that HTTP Context is available!
            if (context.ActionContext.HttpContext == null)
            {
                throw new InvalidOperationException("HTTP Context is not available.");
            }

            IOptions<ChameOptions> options = context.ActionContext.HttpContext.RequestServices.GetService(typeof(IOptions<ChameOptions>)) as IOptions<ChameOptions>;          
            if (options == null || options.Value == null)
            {
                throw new InvalidOperationException("IOptions<ChameOptions> is not available.");
            }

            IChameThemeResolver themeResolver = options.Value.ThemeResolver;
            if (themeResolver == null)
            {
                throw new InvalidOperationException("IChameThemeResolver is not available.");

            }

            // Resolve theme that will be used when loading Razor views.
            string theme = themeResolver.GetTheme(new ChameRazorThemeResolveContext(
                context.ActionContext.HttpContext,
                context.ViewName,
                context.ControllerName,
                context.PageName,
                context.AreaName,
                context.IsMainPage,
                context.Values));

            if (string.IsNullOrEmpty(theme))
            {
                throw new InvalidOperationException("Theme not found.");
            }

            context.Values[ThemeKey] = theme;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            string theme;
            if (context.Values.TryGetValue(ThemeKey, out theme))
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
