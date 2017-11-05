using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chame.Caching;

namespace Chame.ContentLoaders.FileSystem
{
    public class FileSystemLoaderOptions
    {
        public FileSystemLoaderOptions()
        {
            CachingMode = CachingModes.EnabledButDisabledOnDev;
            CacheAbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 1, 0);
        }

        /// <summary>
        /// The root directory. This should be an absolute path.
        /// </summary>
        public string Root { get; set; }

        /// <summary>
        /// Caching mode, the default value is <see cref="CachingModes.EnabledButDisabledOnDev"/>.
        /// </summary>
        public CachingModes CachingMode { get; set; }

        /// <summary>
        /// An absolute expiration time for caching, relative to now. The default value is 1 minute.
        /// </summary>
        public TimeSpan CacheAbsoluteExpirationRelativeToNow { get; set; }

    }
}
