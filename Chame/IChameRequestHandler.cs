using System.Threading.Tasks;

namespace Chame
{
    public interface IChameRequestHandler
    {
        Task HandleAsync(ChameContext context);
    }
}
