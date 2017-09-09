using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chame.Loaders;

namespace Chame
{
    public class ChameOptions
    {
        public const string DefaultThemeName = "default";

        public delegate void ContentLoaderSorterDelegate(IContentLoader[] loaders);

        public static ChameOptions CreateDefault()
        {
            return new ChameOptions
            {
                Theme = DefaultThemeName,
                ETagEnabled = true,
                SortContentLoaders = SortContentLoadersByPriority
            };
        }

        public virtual string Theme { get; set; }

        public virtual bool ETagEnabled { get; set; }

        public virtual ContentLoaderSorterDelegate SortContentLoaders { get; set; }

        /// <summary>
        /// This is the default sorter for IContentLoader implementations.
        /// Implementations are sorted by priority.
        /// </summary>
        private static void SortContentLoadersByPriority(IContentLoader[] loaders)
        {
            Array.Sort(loaders, (loader1, loader2) => loader1.Priority.CompareTo(loader2.Priority));
        }

    }
}
