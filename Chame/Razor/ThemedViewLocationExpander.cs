using Chame.Themes;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Chame.Internal;

namespace Chame.Razor
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private const string ViewLocationExpanderContextKey = "__Chame.Razor.ThemedViewLocationExpander";

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
            ChameUtility utils = context.ActionContext.HttpContext.RequestServices.GetRequiredService<ChameUtility>();

            // resolve theme
            ITheme theme = utils.GetTheme(context.ActionContext.HttpContext);
            context.Values[ViewLocationExpanderContextKey] = theme.GetName();
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(ViewLocationExpanderContextKey, out string themeName))
            {
                IEnumerable<string> themedViewLocations = _viewLocationTemplates.Select(template => string.Format(template, themeName));

                IEnumerable<string> newViewLocations = themedViewLocations.Concat(viewLocations);

                return newViewLocations.ToArray();
            }

            return viewLocations;
        }
    }
}
