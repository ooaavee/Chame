using System;
using Chame;
using Chame.FileSystem;
using Chame.FileSystem.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
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

            // my services
            services.TryAddSingleton<ChameContextFactory, ChameContextFactory>();
            services.TryAddSingleton<ChameContextHandler, ChameContextHandler>();

            return services;
        }

        public static IServiceCollection AddChameFileSystemLoader(this IServiceCollection services, Action<ContentLoaderOptions> configureOptions = null)
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
            services.Configure<ContentLoaderOptions>(configureOptions);

            // my services
            services.TryAddSingleton<IJsLoader, ContentLoader>();
            services.TryAddSingleton<ICssLoader, ContentLoader>();
            services.TryAddSingleton<Cache>();
            services.TryAddSingleton<ThemeBundleResolver>();

            // framework services
            services.AddMemoryCache();

            return services;
        }

    }
}