using System;

namespace Chame.Caching
{
    public struct CachingSupport
    {
        /// <summary>
        /// Caching mode, the default value is <see cref="CachingModes.EnabledButDisabledOnDev"/>.
        /// </summary>
        public CachingModes Mode { get; set; }

        /// <summary>
        /// An absolute expiration time for caching, relative to now. The default value is 1 minute.
        /// </summary>
        public TimeSpan AbsoluteExpirationRelativeToNow { get; set; }
    }
}
