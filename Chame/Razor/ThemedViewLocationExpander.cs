using System;
using System.Collections.Generic;
using System.Linq;
using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.Razor
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private const string Key = "__Chame.Razor.ThemedViewLocationExpander";

        /// <summary>
        /// View location templates.
        /// </summary>
        private readonly IList<string> _viewLocationTemplates;

        public ThemedViewLocationExpander(RazorThemeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _viewLocationTemplates = options.ViewLocationTemplates;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // current http context
            HttpContext httpContext = context.ActionContext.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            // logger
            ILogger<ThemedViewLocationExpander> logger = httpContext.RequestServices.GetRequiredService<ILogger<ThemedViewLocationExpander>>();

            // options
            IOptions<ContentLoaderOptions> options = httpContext.RequestServices.GetRequiredService<IOptions<ContentLoaderOptions>>();

            // resolve theme
            ITheme theme = ThemeHelper.ResolveTheme(new RazorThemeResolvingContext(context), options.Value.ThemeResolver, options.Value.DefaultTheme);
            if (theme == null)
            {
                var message = "Could not resolve a theme.";
                logger.LogCritical(message);
                throw new InvalidOperationException(message);
            }

            string themeName = theme.GetName();

            context.Values[Key] = themeName;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(Key, out string themeName))
            {
                IEnumerable<string> themeLocations = _viewLocationTemplates.Select(template => string.Format(template, themeName));
                viewLocations = themeLocations.Concat(viewLocations);
            }
            return viewLocations;
        }

    }
}
