using Microsoft.Extensions.DependencyInjection;

namespace Chame
{
    internal sealed class ChameBuilder : IChameBuilder
    {
        public IServiceCollection Services { get; set; }
    }
}