using System.Threading.Tasks;

namespace Chame.Loaders
{
    public interface IContentLoader
    {
        int Priority { get; }

        Task<ResponseContent> LoadAsync(ChameContext context);
    }
}
