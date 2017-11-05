using System;
using System.Collections.Generic;

namespace Chame.ContentLoaders
{
    public class DefaultContentLoaderSorter : IContentLoaderSorter
    {
        public void Sort(List<IContentLoader> loaders)
        {
            if (loaders == null)
            {
                throw new ArgumentNullException(nameof(loaders));
            }

            loaders.Sort((item1, item2) => item1.Priority.CompareTo(item2.Priority));
        }
    }
}