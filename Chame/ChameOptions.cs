using System;
using Chame.Extensions;

namespace Chame
{
    public class ChameOptions : IChameEvents
    {
        public const string DefaultThemeName = "default";

        public ChameOptions()
        {
            Events = this;
            Events.ContentLoaderSorter = SortContentLoadersByPriority;
            DefaultTheme = DefaultThemeName;        
            UseETag = true;
        }

        public IChameEvents Events { get; set; }

        public string DefaultTheme { get; set; }
        
        public bool UseETag { get; set; }
        
        Func<ThemeResolverEventArgs, string> IChameEvents.ThemeResolver { get; set; }

        Action<IContentLoader[]> IChameEvents.ContentLoaderSorter { get; set; }

        /// <summary>
        /// The default sorter for IContentLoader implementations -> implementations are sorted by priority.
        /// </summary>
        private static void SortContentLoadersByPriority(IContentLoader[] loaders)
        {
            Array.Sort(loaders, (loader1, loader2) => loader1.Priority.CompareTo(loader2.Priority));
        }
    }
}