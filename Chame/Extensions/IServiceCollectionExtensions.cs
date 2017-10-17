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

            ChameBuilder builder = new ChameBuilder {Services = services};

            // options
            builder.Services.Configure<ChameOptions>(configureOptions);

            // my services
            builder.Services.TryAddSingleton<ChameContextFactory, ChameContextFactory>();
            builder.Services.TryAddSingleton<RequestProcessor, RequestProcessor>();

            return builder;
        }
    }
}