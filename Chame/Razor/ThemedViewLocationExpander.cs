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

        public ThemedViewLocationExpander(IList<string> viewLocationTemplates)
        {
            if (viewLocationTemplates == null)
            {
                throw new ArgumentNullException(nameof(viewLocationTemplates));
            }
            _viewLocationTemplates = viewLocationTemplates;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // logger
            ILogger<ThemedViewLocationExpander> logger = context.ActionContext.HttpContext.RequestServices.GetRequiredService<ILogger<ThemedViewLocationExpander>>();

            // options
            IOptions<ContentLoaderOptions> options = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IOptions<ContentLoaderOptions>>();

            // theme
            ITheme theme = GetTheme(context.ActionContext.HttpContext, options.Value.DefaultTheme);
            if (theme == null)
            {
                var message = "Could not resolve a theme.";
                logger.LogCritical(message);
                throw new InvalidOperationException(message);
            }

            context.Values[Key] = theme.GetName();
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(Key, out string themeName))
            {
                IEnumerable<string> themedViewLocations = _viewLocationTemplates.Select(template => string.Format(template, themeName));
                IEnumerable<string> newViewLocations = themedViewLocations.Concat(viewLocations);
                return newViewLocations.ToArray();
            }
            return viewLocations;
        }

        private static ITheme GetTheme(HttpContext httpContext, ITheme defaultTheme)
        {
            ITheme theme = null;
            IThemeResolver resolver = httpContext.RequestServices.GetService<IThemeResolver>();
            if (resolver != null)
            {
                theme = resolver.GetTheme(httpContext);
            }
            return theme ?? defaultTheme;
        }

    }
}
