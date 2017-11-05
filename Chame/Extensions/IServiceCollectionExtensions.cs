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
        public static IContentLoaderBuilder AddContentLoader(this IServiceCollection services, Action<ContentLoaderOptions> setupAction = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                setupAction = options => { };
            }

            // options
            services.Configure(setupAction);

            // my services
            services.TryAddSingleton<ContentCache>();

            // framework services
            services.AddMemoryCache();

            return new DefaultContentLoaderBuilder(services);
        }
    }
}