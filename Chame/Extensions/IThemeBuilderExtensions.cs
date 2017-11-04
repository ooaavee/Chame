using System;
using Chame;
using Chame.ContentLoaders;
using Chame.ContentLoaders.JsAndCssFiles;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IThemeBuilder extension methods
    /// </summary>
    public static class IThemeBuilderExtensions
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
            builder.Services.Configure<JsAndCssFileLoaderOptions>(setupAction);

            // my services
            builder.Services.TryAddSingleton<IContentLoader, JsAndCssFileLoader>();

            // framework services
            builder.Services.AddMemoryCache();

            return builder;
        }
    }
}
