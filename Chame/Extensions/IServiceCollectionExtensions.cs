using System;
using Chame;
using Chame.Caching;
using Chame.ContentLoaders;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection extension methods
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IContentLoaderBuilder AddContentLoader(this IServiceCollection services, Action<ContentLoaderOptions> configureOptions = null)
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
            services.TryAddSingleton<ContentCache>();

            return new DefaultContentLoaderBuilder(services);
        }
    }
}