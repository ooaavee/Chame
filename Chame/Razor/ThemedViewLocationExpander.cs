using Chame.Internal;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chame.Razor
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private const string Key = "__Chame.Razor.ThemedViewLocationExpander";

        /// <summary>
        /// View location templates.
        /// </summary>
        private readonly IEnumerable<string> _viewLocations;

        public ThemedViewLocationExpander(IEnumerable<string> viewLocations)
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

            ChameUtility utils = httpContext.FindChameUtility();

            ITheme theme = utils.GetTheme(httpContext);

            context.Values[Key] = theme.GetName();
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(Key, out string themeName))
            {
                IEnumerable<string> themedViewLocations = _viewLocations.Select(x => string.Format(x, themeName));

                IEnumerable<string> newViewLocations = themedViewLocations.Concat(viewLocations);

                viewLocations = newViewLocations.ToArray();
            }

            return viewLocations;
        }
    }
}
