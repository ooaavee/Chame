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
            SupportETag = true;
            ContentLoaderSorter = new DefaultContentLoaderSorter();
            ContentModel = new DefaultContentModel();
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
        /// Content loaders
        /// </summary>
        public List<IContentLoader> ContentLoaders { get; } = new List<IContentLoader>();

        /// <summary>
        /// Indicates if we should support HTTP ETags if possible. The default value is true.
        /// </summary>
        public bool SupportETag { get; set; }

        /// <summary>
        /// An execution order sorter for <see cref="IContentLoader"/> implementations.
        /// </summary>
        public IContentLoaderSorter ContentLoaderSorter { get; set; }

        public IContentModel ContentModel { get; set; }

     

    }
}