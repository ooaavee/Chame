using System;

namespace Chame.Caching
{
    public struct CachingSupport
    {
        /// <summary>
        /// Caching mode.
        /// </summary>
        public CachingModes Mode { get; set; }

        /// <summary>
        /// An absolute expiration time for caching, relative to now.
        /// </summary>
        public TimeSpan AbsoluteExpirationRelativeToNow { get; set; }
    }
}
