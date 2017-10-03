using System;
using Chame;
using Chame.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IChameBuilderExtensions
    {
        public static IChameBuilder AddFileSystemLoader(this IChameBuilder builder, Action<FileSystemLoaderOptions> configureOptions = null)
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
            builder.Services.Configure<FileSystemLoaderOptions>(configureOptions);

            // my services
            builder.Services.TryAddSingleton<IJsLoader, FileSystemLoader>();
            builder.Services.TryAddSingleton<ICssLoader, FileSystemLoader>();
            builder.Services.TryAddSingleton<ContentCache>();
            builder.Services.TryAddSingleton<ThemeResolver>();

            // framework services
            builder.Services.AddMemoryCache();

            return builder;
        }
    }
}
