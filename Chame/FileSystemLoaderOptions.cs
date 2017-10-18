using System;
using Chame.Models;
using Microsoft.AspNetCore.Hosting;

namespace Chame
{
    public class FileSystemLoaderOptions
    {
        public FileSystemLoaderOptions()
        {
            UseContentSchemaFile = true;
            ContentSchemaFile = @"\chame.json";
            CachingMode = CachingModes.DisabledOnDevelopmentOtherwiseEnabled;
            CacheAbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 1, 0);
        }

        /// <summary>
        /// A path to the theme container file.
        /// </summary>
        public string ContentSchemaFile { get; set; }

        /// <summary>
        /// Should we use the theme container file.
        /// </summary>
        public bool UseContentSchemaFile { get; set; }

        /// <summary>
        /// A function for loading <see cref="ContentSchema"/> objects. This will be invoked if <see cref="UseContentSchemaFile"/> is false.
        /// </summary>
        public Func<ContentLoadingContext, ContentSchema> ContentSchemaResolver { get; set; }

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
