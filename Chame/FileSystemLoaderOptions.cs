using System;
using Chame.Models;
using Microsoft.AspNetCore.Hosting;

namespace Chame
{
    public class FileSystemLoaderOptions
    {
        public FileSystemLoaderOptions()
        {
            UseThemeContainerFile = true;
            ThemeContainerFile = @"\chame-files.json";
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
        /// A function for loading <see cref="ThemeContainer"/> objects. This will be invoked if <see cref="UseThemeContainerFile"/> is false.
        /// </summary>
        public Func<ThemeContainer> ThemeContainerGetter { get; set; }

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
