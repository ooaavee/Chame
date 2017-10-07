using System;
using Chame;
using Chame.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IChameBuilder extension methods
    /// </summary>
    public static class IChameBuilderExtensions
    {
        public static IChameBuilder AddFileSystemLoader(this IChameBuilder builder, Action<ChameFileSystemLoaderOptions> configureOptions = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configureOptions == null)
            {
                configureOptions = options => { };
            }

            // options
            builder.Services.Configure<ChameFileSystemLoaderOptions>(configureOptions);

            // my services
            builder.Services.TryAddSingleton<IJsLoader, FileSystemLoader>();
            builder.Services.TryAddSingleton<ICssLoader, FileSystemLoader>();
            builder.Services.TryAddSingleton<ChameMemoryCache>();
            builder.Services.TryAddSingleton<ThemeResolver>();

            // framework services
            builder.Services.AddMemoryCache();

            return builder;
        }

        public static IChameBuilder AddRazorViews(this IChameBuilder builder, Action<ChameRazorOptions> configureOptions = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configureOptions == null)
            {
                configureOptions = options => { };
            }

            // options
            builder.Services.Configure<ChameRazorOptions>(configureOptions);

            // my services
            builder.Services.TryAddSingleton<IChameRazorThemeResolver, RazorThemeResolver>();

            return builder;
        }

    }
}
