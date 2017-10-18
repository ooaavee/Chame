using System;
using Chame;
using Chame.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection extension methods
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IChameBuilder AddChame(this IServiceCollection services, Action<ChameOptions> configureOptions = null)
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
            services.TryAddSingleton<ContentCache>();

            return new ChameBuilder {Services = services};
        }

        private class ChameBuilder : IChameBuilder
        {
            public IServiceCollection Services { get; set; }
        }

    }
}