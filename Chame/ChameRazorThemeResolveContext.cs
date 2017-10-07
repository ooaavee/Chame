using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameRazorThemeResolveContext
    {
        public ChameRazorThemeResolveContext(
            HttpContext httpContext,
            string viewName,
            string controllerName,
            string pageName,
            string areaName,
            bool isMainPage,
            IDictionary<string, string> values)
        {
            HttpContext = httpContext;
            ViewName = viewName;
            ControllerName = controllerName;
            PageName = pageName;
            AreaName = areaName;
            IsMainPage = isMainPage;
            Values = values;
        }

        /// <summary>
        /// HTTP Context
        /// </summary>
        public HttpContext HttpContext { get;  }

        /// <summary>
        /// Gets the view name.
        /// </summary>
        public string ViewName { get;  }

        /// <summary>
        /// Gets the controller name.
        /// </summary>
        public string ControllerName { get;  }

        /// <summary>
        /// Gets the page name. This will be the value of the page route value when rendering 
        /// a Page from the Razor Pages framework. This value will be null if rendering a 
        /// view as the result of a controller.
        /// </summary>
        public string PageName { get;  }

        /// <summary>
        /// Gets the area name.
        /// </summary>
        public string AreaName { get;  }

        /// <summary>
        /// Determines if the page being found is the main page for an action.
        /// </summary>
        public bool IsMainPage { get;  }

        /// <summary>
        /// Gets the System.Collections.Generic.IDictionary`2 that is populated with
        /// values as part of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.PopulateValues(Microsoft.AspNetCore.Mvc.Razor.ViewLocationExpanderContext).
        /// </summary>
        public IDictionary<string, string> Values { get;  }

    }
}