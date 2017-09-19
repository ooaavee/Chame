using System;
using Chame.Extensions;
using Chame.Loaders;

namespace Chame
{
    public class ChameOptions
    {
        public const string DefaultThemeName = "default";

        public ChameOptions()
        {
            Events = new DefaultChameEvents { ContentLoaderSorter = SortContentLoadersByPriority };
            DefaultTheme = DefaultThemeName;
            UseETag = true;
        }

        public IChameEvents Events { get; set; }

        public string DefaultTheme { get; set; }
        
        public bool UseETag { get; set; }
        
        /// <summary>
        /// This is the default sorter for IContentLoader implementations.
        /// Implementations are sorted by priority.
        /// </summary>
        private static void SortContentLoadersByPriority(IContentLoader[] loaders)
        {
            Array.Sort(loaders, (loader1, loader2) => loader1.Priority.CompareTo(loader2.Priority));
        }

        private sealed class DefaultChameEvents : IChameEvents
        {
            public Func<ThemeResolverEventArgs, string> ThemeResolver { get; set; }
            public Action<IContentLoader[]> ContentLoaderSorter { get; set; }
        }
    }
}
