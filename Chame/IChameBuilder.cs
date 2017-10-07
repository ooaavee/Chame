using Microsoft.Extensions.DependencyInjection;

namespace Chame
{
    public interface IChameBuilder
    {
        IServiceCollection Services { get; }
    }
}
