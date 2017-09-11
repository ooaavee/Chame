using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chame.Extensions;
using Chame.Loaders;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameOptions
    {
        public const string DefaultThemeName = "default";

        public static ChameOptions CreateDefault()
        {
            ChameOptions options = new ChameOptions();

            options.Events = new DefaultChameEvents();
            options.Events.ContentLoaderSorter = SortContentLoadersByPriority;
            options.DefaultTheme = DefaultThemeName;
            options.UseETag = true;

            return options;
        }

        public virtual IChameEvents Events { get; set; }

        public virtual string DefaultTheme { get; set; }
        
        public virtual bool UseETag { get; set; }

        
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
