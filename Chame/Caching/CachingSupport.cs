using System;
using Microsoft.AspNetCore.Hosting;

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

        public bool IsEnabled(IHostingEnvironment env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            switch (Mode)
            {
                case CachingModes.Disabled:
                    return false;
                case CachingModes.Enabled:
                    return true;
                case CachingModes.EnabledButDisabledOnDev:
                    if (env.IsDevelopment())
                    {
                        return false;
                    }
                    return true;
                default:
                    return false;
            }
        }

    }
}
