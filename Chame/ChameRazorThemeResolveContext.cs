using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameRazorThemeResolveContext
    {
        /// <summary>
        /// HTTP Context
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets the view name.
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// Gets the controller name.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// Gets the page name. This will be the value of the page route value when rendering 
        /// a Page from the Razor Pages framework. This value will be null if rendering a 
        /// view as the result of a controller.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Gets the area name.
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// Determines if the page being found is the main page for an action.
        /// </summary>
        public bool IsMainPage { get; set; }

        /// <summary>
        /// Gets or sets the System.Collections.Generic.IDictionary`2 that is populated with
        /// values as part of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.PopulateValues(Microsoft.AspNetCore.Mvc.Razor.ViewLocationExpanderContext).
        /// </summary>
        public IDictionary<string, string> Values { get; set; }

    }
}