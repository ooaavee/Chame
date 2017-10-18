using System.Threading.Tasks;

namespace Chame
{
    public interface IContentLoader
    {
        /// <summary>
        /// Content loader priority. 
        /// An execution order of content loaders are sorted by this property. 
        /// This is only meaningful if there are more than one content loaders!
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Loads content.
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>response</returns>
        Task<ResponseContent> LoadContentAsync(ContentLoadingContext context);
    }

    public interface IJsContentLoader : IContentLoader
    {
    }

    public interface ICssContentLoader : IContentLoader
    {
    }

}
