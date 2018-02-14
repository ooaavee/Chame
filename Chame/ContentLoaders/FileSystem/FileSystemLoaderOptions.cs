using System;
using Chame.Caching;

namespace Chame.ContentLoaders.FileSystem
{
    public class FileSystemLoaderOptions
    {
        public FileSystemLoaderOptions()
        {
            Caching = new CachingSupport
            {
                Mode = CachingModes.Disabled,
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 1, 0)
            };
        }

        /// <summary>
        /// The root directory. This should be an absolute path.
        /// </summary>
        public string Root { get; set; }

        public CachingSupport Caching { get; set; }
    }
}
