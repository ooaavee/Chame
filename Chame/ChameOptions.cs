using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chame
{
    public class ChameOptions
    {
        private const string DefaultThemeName = "default";

        /// <summary>
        /// Initializes a new instance of the ChameOptions class.
        /// </summary>
        public ChameOptions()
        {
            ContentLoaderSorter = SortContentLoadersByPriority;
            DefaultTheme = DefaultThemeName;        
            SupportETag = true;            
        }

        /// <summary>
        /// The name of the default theme that should be used.
        /// </summary>
        public string DefaultTheme { get; set; }

        /// <summary>
        /// An extension point for loading JavaScript content.
        /// </summary>
        public Func<ContentLoadingContext, Task<ResponseContent>> JsLoader { get; set; }

        /// <summary>
        /// An extension point for loading CSS content.
        /// </summary>
        public Func<ContentLoadingContext, Task<ResponseContent>> CssLoader { get; set; }

        /// <summary>
        /// Resolves the theme that should be used. If not set, the <see cref="DefaultTheme"/> will be used.
        /// </summary>
        public IThemeResolver ThemeResolver { get; set; }

        /// <summary>
        /// Indicates if we should support HTTP ETags if possible.
        /// </summary>
        public bool SupportETag { get; set; }

        /// <summary>
        /// An execution order sorter for <see cref="IContentLoader"/> implementations.
        /// </summary>
        public Action<List<IContentLoader>> ContentLoaderSorter { get; set; }

        /// <summary>
        /// The default sorter for IContentLoader implementations -> implementations are sorted by priority.
        /// </summary>
        private static void SortContentLoadersByPriority(List<IContentLoader> loaders)
        {
            loaders.Sort((loader1, loader2) => loader1.Priority.CompareTo(loader2.Priority));
        }
    }
}