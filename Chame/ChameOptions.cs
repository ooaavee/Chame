using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chame.Loaders;

namespace Chame
{
    public class ChameOptions
    {
        public delegate IContentLoader[] ContentLoaderSorterDelegate(IContentLoader[] loaders);

        public static ChameOptions CreateDefault()
        {
            return new ChameOptions
            {
                Theme = "default",
                SortContentLoaders = ChameOptions.SortContentLoadersByPriority
            };
        }

        public virtual string Theme { get; set; }

        public virtual ContentLoaderSorterDelegate SortContentLoaders { get; set; }

        /// <summary>
        /// This is the default sorter for IContentLoader implementations.
        /// Implementations are sorted by priority.
        /// </summary>
        private static IContentLoader[] SortContentLoadersByPriority(IContentLoader[] loaders)
        {
            if (loaders == null)
            {
                throw new ArgumentNullException(nameof(loaders));
            }

            Array.Sort(loaders, (loader1, loader2) => loader1.Priority.CompareTo(loader2.Priority));
            return loaders;
        }

    }
}
