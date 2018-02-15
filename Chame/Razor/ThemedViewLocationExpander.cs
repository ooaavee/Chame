using Chame.Internal;
using Chame.Themes;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Chame.Razor
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private const string Key = "__Chame.Razor.ThemedViewLocationExpander";

        /// <summary>
        /// View location templates.
        /// </summary>
        private readonly IList<string> _viewLocations;

        public ThemedViewLocationExpander(IList<string> viewLocations)
        {
            if (viewLocations == null)
            {
                throw new ArgumentNullException(nameof(viewLocations));
            }
            _viewLocations = viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            HttpContext httpContext = context.ActionContext.HttpContext;

            // resolve a theme that will be used when loading Razor views
            ITheme theme = httpContext.ChameUtility().GetTheme(httpContext);

            context.Values[Key] = theme.GetName();
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(Key, out string themeName))
            {
                IEnumerable<string> themedViewLocations = _viewLocations.Select(template => string.Format(template, themeName));

                IEnumerable<string> newViewLocations = themedViewLocations.Concat(viewLocations);

                viewLocations = newViewLocations.ToArray();
            }

            return viewLocations;
        }
    }
}
