using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chame.ContentLoaders
{
    public interface IContentLoader
    {
        /// <summary>
        /// Content loader priority. 
        /// An execution order of content loaders are sorted by this property. 
        /// This is only meaningful if there are more than one content loaders!
        /// </summary>
        double Priority { get; }

        /// <summary>
        /// Content-type extensions that are supported by the content loader.
        /// </summary>
        /// <returns>supported content-type extensions</returns>
        IEnumerable<string> ContentTypeExtensions();

        /// <summary>
        /// Loads content.
        /// </summary>
        /// <param name="context">A context object that tells you what was requested.</param>
        /// <returns>loaded content</returns>
        Task<Content> LoadContentAsync(ContentLoadingContext context);
    }   
}
