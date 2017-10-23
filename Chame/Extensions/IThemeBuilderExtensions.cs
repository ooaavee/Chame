using System;
using Chame;
using Chame.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IThemeBuilder extension methods
    /// </summary>
    public static class IThemeBuilderExtensions
    {
        public static IThemeBuilder AddFileSystemLoader(this IThemeBuilder builder, Action<FileSystemContentLoaderOptions> configureOptions = null)
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
            builder.Services.Configure<FileSystemContentLoaderOptions>(configureOptions);

            // my services
            builder.Services.TryAddSingleton<IContentLoader, FileSystemContentLoader>();

            // framework services
            builder.Services.AddMemoryCache();

            return builder;
        }
    }
}
