using System.Threading.Tasks;

namespace Chame
{
    public interface IChameRequestHandler
    {
        Task<bool> HandleAsync(ChameContext context);
    }
}
