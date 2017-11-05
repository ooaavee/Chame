using System.Collections.Generic;

namespace Chame.ContentLoaders
{
    /// <summary>
    /// An execution order sorter for <see cref="IContentLoader"/> instances.
    /// </summary>
    public interface IContentLoaderSorter
    {
        void Sort(List<IContentLoader> loaders);
    }
}
