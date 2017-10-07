using System;
using Chame.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameFileSystemLoaderOptions
    {
        public ChameFileSystemLoaderOptions()
        {
            UseThemeContainerFile = true;
            ThemeContainerFile = @"\chame.json";
            CachingMode = CachingModes.DisabledOnDevelopmentOtherwiseEnabled;
            CacheAbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 1, 0);
        }

        /// <summary>
        /// A path to the theme container file.
        /// </summary>
        public string ThemeContainerFile { get; set; }

        /// <summary>
        /// Should we use the theme container file.
        /// </summary>
        public bool UseThemeContainerFile { get; set; }

        /// <summary>
        /// A function for loading <see cref="ContentFileThemeContainer"/> objects. This will be invoked if <see cref="UseThemeContainerFile"/> is false.
        /// </summary>
        public Func<ChameContext, ContentFileThemeContainer> ThemeContainerLoader { get; set; }

        /// <summary>
        /// Caching mode, the default value is <see cref="CachingModes.DisabledOnDevelopmentOtherwiseEnabled"/>.
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
            switch (CachingMode)
            {
                case CachingModes.Disabled:
                    return false;

                case CachingModes.Enabled:
                    return true;

                case CachingModes.DisabledOnDevelopmentOtherwiseEnabled:
                    if (env.IsDevelopment())
                    {
                        return false;
                    }
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
