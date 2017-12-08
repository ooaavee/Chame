using System.Threading.Tasks;

namespace Chame.ContentLoaders
{
    public interface IContentNotFoundCallback
    {
        Task<byte[]> GetDefaultContentAsync(ContentLoadingContext context);
    }
}
