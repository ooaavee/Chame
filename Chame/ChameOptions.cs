using System;
using Chame.Services;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameOptions
    {
        public const string DefaultThemeName = "default";

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
        /// Indicates if we should support HTTP ETags if possible.
        /// </summary>
        public bool SupportETag { get; set; }

        /// <summary>
        /// Resolves the theme that should be used. If not set, the <see cref="DefaultTheme"/> will be used.
        /// </summary>
        public Func<ThemeResolverEventArgs, string> ThemeResolver { get; set; }

        /// <summary>
        /// An execution order sorter for <see cref="IContentLoader"/> implementations.
        /// </summary>
        public Action<IContentLoader[]> ContentLoaderSorter { get; set; }

        /// <summary>
        /// The default sorter for IContentLoader implementations -> implementations are sorted by priority.
        /// </summary>
        private static void SortContentLoadersByPriority(IContentLoader[] loaders)
        {
            Array.Sort(loaders, (loader1, loader2) => loader1.Priority.CompareTo(loader2.Priority));
        }
    }

    public class ThemeResolverEventArgs
    {
        public ThemeResolverEventArgs(HttpContext httpContext, ContentCategory category, string filter)
        {
            HttpContext = httpContext;
            Category = category;
            Filter = filter;
        }

        /// <summary>
        /// Http context
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// What kind of content was requested
        /// </summary>
        public ContentCategory Category { get; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public string Filter { get; }
    }

}