using System;
using Chame;
using Chame.ContentLoaders;
using Chame.ContentLoaders.FileSystem;
using Chame.ContentLoaders.JsAndCssFiles;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IContentLoaderBuilder extension methods
    /// </summary>
    public static class IContentLoaderBuilderExtensions
    {
        public static IContentLoaderBuilder AddJsAndCssFileLoader(this IContentLoaderBuilder builder, Action<JsAndCssFileLoaderOptions> setupAction = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                setupAction = options => { };
            }

            // options
            builder.Services.Configure(setupAction);

            // my services
            builder.Services.TryAddSingleton<IContentLoader, JsAndCssFileLoader>();

            return builder;
        }

        public static IContentLoaderBuilder AddFileSystemLoaders(this IContentLoaderBuilder builder, Action<FileSystemLoaderOptions> setupAction = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                setupAction = options => { };
            }

            // options
            builder.Services.Configure(setupAction);

            // my services
            builder.Services.TryAddSingleton<IContentLoader, FileSystemLoader>();

            return builder;
        }

    }
}
