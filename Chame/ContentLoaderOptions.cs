using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chame
{
    public class ContentLoaderOptions
    {
        /// <summary>
        /// Initializes a new instance of the ContentLoaderOptions class.
        /// </summary>
        public ContentLoaderOptions()
        {
            DefaultTheme = "default";
            VirtualPathForJsRequests = "/chame-js-loader";
            VirtualPathForCssRequests = "/chame-css-loader";
            SupportETag = true;
            ContentLoaderSorter = delegate(List<IContentLoader> items)
            {
                items.Sort((item1, item2) => item1.Priority.CompareTo(item2.Priority));
            };
        }

        /// <summary>
        /// The name of the default theme that should be used. The default value is 'default'.
        /// </summary>
        public string DefaultTheme { get; set; }

        /// <summary>
        /// Resolves the theme that should be used. If not set, the <see cref="DefaultTheme"/> will be used.
        /// </summary>
        public IThemeResolver ThemeResolver { get; set; }

        /// <summary>
        /// An extension point for loading JavaScript content. If set, will be invoked before registered <see cref="IJsContentLoader"/> services.
        /// </summary>
        public Func<ContentLoadingContext, Task<TextContent>> JsLoader { get; set; }

        /// <summary>
        /// An extension point for loading CSS content. If set, will be invoked before registered <see cref="ICssContentLoader"/> services.
        /// </summary>
        public Func<ContentLoadingContext, Task<TextContent>> CssLoader { get; set; }

        /// <summary>
        /// Indicates if we should support HTTP ETags if possible. The default value is true.
        /// </summary>
        public bool SupportETag { get; set; }

        /// <summary>
        /// An execution order sorter for <see cref="IJsContentLoader"/> and <see cref="ICssContentLoader"/> implementations. If not not, the default 
        /// implementation will be used, which sorts implementations by <see cref="IContentLoader.Priority"/> property.
        /// </summary>
        public Action<List<IContentLoader>> ContentLoaderSorter { get; set; }

        public string VirtualPathForJsRequests { get; set; }

        public string VirtualPathForCssRequests { get; set; }

    }
}