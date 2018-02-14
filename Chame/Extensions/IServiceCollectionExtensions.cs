using Chame;
using Chame.Caching;
using Chame.ContentLoaders;
using Chame.ContentLoaders.FileSystem;
using Chame.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection extension methods
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddContentLoader(this IServiceCollection services, Action<ContentLoaderOptions> setupAction = null)
        { 
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                setupAction = options => { };
            }

            // options
            services.Configure(setupAction);

            // my services
            services.TryAddSingleton<ContentCache>();
            services.TryAddSingleton<ChameUtility>();
            services.TryAddSingleton<IChameService, ChameService>();

            // framework services
            services.AddMemoryCache();

            return services;
        }

        public static IServiceCollection AddFileSystemLoader(this IServiceCollection services, Action<FileSystemLoaderOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            // options
            services.Configure(setupAction);

            // my services
            services.TryAddSingleton<IContentLoader, FileSystemLoader>();

            return services;
        }
    }
}