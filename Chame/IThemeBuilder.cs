using Microsoft.Extensions.DependencyInjection;

namespace Chame
{
    public interface IThemeBuilder
    {
        IServiceCollection Services { get; }
    }

    internal sealed class ThemeBuilder : IThemeBuilder
    {
        public ThemeBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
