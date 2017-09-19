using System;
using Chame;
using Chame.Loaders;
using Chame.Loaders.FileSystem;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChame(this IServiceCollection services, Action<ChameOptions> configureOptions = null)
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
            services.Configure<ChameOptions>(configureOptions);

            // own services
            services.TryAddSingleton<IChameContextFactory, DefaultChameContextFactory>();
            services.TryAddSingleton<IChameRequestHandler, DefaultChameRequestHandler>();

            return services;
        }

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

            // own services
            services.TryAddSingleton<IJsLoader, FileSystemContentLoader>();
            services.TryAddSingleton<ICssLoader, FileSystemContentLoader>();
            services.TryAddSingleton<Cache>();
            services.TryAddSingleton<ThemeBundleResolver>();

            // framework services
            services.AddMemoryCache();

            return services;
        }
    }
}