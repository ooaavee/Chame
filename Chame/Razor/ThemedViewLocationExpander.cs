using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chame.Razor
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private const string ViewLocationExpanderContextKey = "__Chame.Razor.ThemedViewLocationExpander";
        private const string HttpContextItemsKey = "__Chame.Razor.ThemedViewLocationExpander.HttpContext.Items";

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
            // options
            var options = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IOptions<ContentLoaderOptions>>();

            // theme
            var theme = GetTheme(context.ActionContext.HttpContext, options.Value.DefaultTheme);
            if (theme == null)
            {
                throw new InvalidOperationException("Could not resolve a theme.");
            }

            context.Values[ViewLocationExpanderContextKey] = theme.GetName();
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(ViewLocationExpanderContextKey, out string themeName))
            {
                var themedViewLocations = _viewLocationTemplates.Select(template => string.Format(template, themeName));
                var newViewLocations = themedViewLocations.Concat(viewLocations);
                return newViewLocations.ToArray();
            }
            return viewLocations;
        }

        private static ITheme GetTheme(HttpContext httpContext, ITheme defaultTheme)
        {
            if (TryGetThemeFromHttpContext(httpContext, out ITheme theme))
            {
                return theme;
            }

            var resolver = httpContext.RequestServices.GetService<IThemeResolver>();
            if (resolver != null)
            {
                theme = resolver.GetTheme(httpContext);
            }

            return theme ?? defaultTheme;
        }

        internal static void UseThemeWithHttpContext(ITheme theme, HttpContext httpContext)
        {
            httpContext.Items[HttpContextItemsKey] = theme;
        }

        private static bool TryGetThemeFromHttpContext(HttpContext httpContext, out ITheme theme)
        {
            if (httpContext.Items.TryGetValue(HttpContextItemsKey, out object v))
            {
                theme = (ITheme)v;
                return true;
            }
            theme = null;
            return false;
        }

    }
}
