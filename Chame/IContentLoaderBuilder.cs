using Microsoft.Extensions.DependencyInjection;

namespace Chame
{
    public interface IContentLoaderBuilder
    {
        IServiceCollection Services { get; }
    }
}
