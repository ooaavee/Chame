using System;
using Chame;
using Chame.Loaders;
using Chame.Loaders.FileSystem;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChame(this IServiceCollection services, Action<ChameOptions> setup)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            var options = ChameOptions.CreateDefault();
            setup(options);

            return AddChame(services, options);
        }

        public static IServiceCollection AddChame(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = ChameOptions.CreateDefault();

            return AddChame(services, options);
        }

        public static IServiceCollection AddChame(this IServiceCollection services, ChameOptions options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.TryAddSingleton<ChameOptions>(x => options);
            services.TryAddSingleton<IChameContextFactory, DefaultChameContextFactory>();
            services.TryAddSingleton<IChameRequestHandler, DefaultChameRequestHandler>();

            return services;
        }


        public static IServiceCollection AddChameFileSystemLoader(this IServiceCollection services, Action<FileSystemContentLoaderOptions> setup)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            var options = FileSystemContentLoaderOptions.CreateDefault();
            setup(options);

            return AddChameFileSystemLoader(services, options);
        }

        public static IServiceCollection AddChameFileSystemLoader(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = FileSystemContentLoaderOptions.CreateDefault();

            return AddChameFileSystemLoader(services, options);
        }

        public static IServiceCollection AddChameFileSystemLoader(this IServiceCollection services, FileSystemContentLoaderOptions options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.TryAddSingleton<FileSystemContentLoaderOptions>(x => options);
            services.TryAddSingleton<IJsLoader, FileSystemContentLoader>();
            services.TryAddSingleton<ICssLoader, FileSystemContentLoader>();

            return services;
        }


    }
}
