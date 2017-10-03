using System;
using Microsoft.AspNetCore.Hosting;

namespace Chame.FileSystem
{
    public class ContentLoaderOptions
    {
        public ContentLoaderOptions()
        {
            UseSetupFile = true;
            SetupFilePath = @"\chame.json";
            CachingMode = CachingModes.EnabledButDisabledOnDevelopment;
            CacheAbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 1, 0);
        }

        /// <summary>
        /// A path to the setup file.
        /// </summary>
        public string SetupFilePath { get; set; }

        /// <summary>
        /// Should we use setup file.
        /// </summary>
        public bool UseSetupFile { get; set; }

        /// <summary>
        /// Current caching mode.
        /// </summary>
        public CachingModes CachingMode { get; set; }

        /// <summary>
        /// An absolute expiration time for caching, relative to now.
        /// </summary>
        public TimeSpan CacheAbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Checks if caching is enabled.
        /// </summary>
        internal bool IsCachingEnabled(IHostingEnvironment env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            switch (CachingMode)
            {
                case CachingModes.Disabled:
                    return false;
                case CachingModes.Enabled:
                    return true;
                case CachingModes.EnabledButDisabledOnDevelopment:
                    if (env.IsDevelopment())
                    {
                        return false;
                    }
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Caching modes
        /// </summary>
        public enum CachingModes
        {
            /// <summary>
            /// Caching is disabled.
            /// </summary>
            Disabled,

            /// <summary>
            /// Caching is enabled.
            /// </summary>
            Enabled,

            /// <summary>
            /// Caching is enabled, but disabled when working on development environment.
            /// </summary>
            EnabledButDisabledOnDevelopment
        }

    }
}
