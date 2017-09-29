using System;
using Chame;
using Chame.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChame(this IServiceCollection services, Action<ChameOptions> configureOptions = null)
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
            services.TryAddSingleton<IChameContextFactory, ContextFactory>();
            services.TryAddSingleton<IChameRequestHandler, RequestHandler>();

            return services;
        }
    }
}