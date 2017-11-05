using System;
using Microsoft.Extensions.DependencyInjection;

namespace Chame
{
    public class DefaultContentLoaderBuilder : IContentLoaderBuilder
    {
        public DefaultContentLoaderBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}