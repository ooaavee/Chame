using System.Collections.Generic;

namespace Chame.ContentLoaders
{
    public class ContentLoaderOptions
    {
        public const string ContentLoaderSupportsAll = "*";

        /// <summary>
        /// Initializes a new instance of the ContentLoaderOptions class.
        /// </summary>
        public ContentLoaderOptions()
        {
            DefaultTheme = new DefaultTheme();
            SupportETag = true;
            ContentLoaderSorter = new DefaultContentLoaderSorter();
            ContentModel = new DefaultContentModel();
            RequestPathTemplate = "/chame-loader/{0}";
        }

        /// <summary>
        /// A fallback theme that will be used if there isn't a registered <see cref="IThemeResolver"/> implementation available or 
        /// it returns a null value.
        /// </summary>
        public ITheme DefaultTheme { get; set; }

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
        /// An execution order sorter for <see cref="IContentLoader"/> instances.
        /// </summary>
        public IContentLoaderSorter ContentLoaderSorter { get; set; }

        /// <summary>
        /// Defines supported content.
        /// </summary>
        public IContentModel ContentModel { get; set; }

        /// <summary>
        /// Request path template, like '/chame-loader/{0}'.
        /// </summary>
        public string RequestPathTemplate { get; set; }
    }
}