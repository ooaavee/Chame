using Microsoft.Extensions.DependencyInjection;

namespace Chame
{
    internal class ChameBuilder : IChameBuilder
    {
        public IServiceCollection Services { get; set; }

    }
}