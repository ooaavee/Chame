using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Chame.Razor
{
    public class RazorThemeResolvingContext
    {
        public RazorThemeResolvingContext(ViewLocationExpanderContext context)
        {
            HttpContext = context.ActionContext.HttpContext;
            ViewName = context.ViewName;
            ControllerName = context.ControllerName;
            PageName = context.PageName;
            AreaName = context.AreaName;
            IsMainPage = context.IsMainPage;
            Values = context.Values;
        }

        /// <summary>
        /// The current HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the view name.
        /// </summary>
        public string ViewName { get; }

        /// <summary>
        /// Gets the controller name.
        /// </summary>
        public string ControllerName { get; }

        /// <summary>
        /// Gets the page name. This will be the value of the page route value when rendering 
        /// a Page from the Razor Pages framework. This value will be null if rendering a 
        /// view as the result of a controller.
        /// </summary>
        public string PageName { get; }

        /// <summary>
        /// Gets the area name.
        /// </summary>
        public string AreaName { get; }

        /// <summary>
        /// Determines if the page being found is the main page for an action.
        /// </summary>
        public bool IsMainPage { get; }

        /// <summary>
        /// Gets the System.Collections.Generic.IDictionary`2 that is populated with
        /// values as part of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.PopulateValues(Microsoft.AspNetCore.Mvc.Razor.ViewLocationExpanderContext).
        /// </summary>
        public IDictionary<string, string> Values { get; }
    }
}