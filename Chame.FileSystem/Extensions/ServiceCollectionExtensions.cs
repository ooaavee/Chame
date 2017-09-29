using System;
using Chame;
using Chame.FileSystem;
using Chame.FileSystem.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChameFileSystemLoader(this IServiceCollection services, Action<FileSystemContentLoaderOptions> configureOptions = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                configureOptions = options => { };
            }

            // options
            services.Configure<FileSystemContentLoaderOptions>(configureOptions);

            // my services
            services.TryAddSingleton<IJsLoader, FileSystemLoader>();
            services.TryAddSingleton<ICssLoader, FileSystemLoader>();
            services.TryAddSingleton<Cache>();
            services.TryAddSingleton<ThemeBundleResolver>();

            // framework services
            services.AddMemoryCache();

            return services;
        }
    }
}
