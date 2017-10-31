using System;
using Microsoft.Extensions.DependencyInjection;

namespace Chame
{
    public interface IContentLoaderBuilder
    {
        IServiceCollection Services { get; }
    }

    internal sealed class DefaultContentLoaderBuilder : IContentLoaderBuilder
    {
        public DefaultContentLoaderBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
