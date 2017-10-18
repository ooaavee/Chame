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
            builder.Services.TryAddSingleton<IJsContentLoader, FileSystemContentLoader>();
            builder.Services.TryAddSingleton<ICssContentLoader, FileSystemContentLoader>();
            builder.Services.TryAddSingleton<ThemedContentFileResolver>();

            // framework services
            builder.Services.AddMemoryCache();

            return builder;
        }
    }
}
