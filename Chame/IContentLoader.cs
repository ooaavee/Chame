using System;
using System.Collections.Generic;
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
        double Priority { get; }

        IEnumerable<string> SupportedContentTypes();

        /// <summary>
        /// Loads content.
        /// </summary>
        /// <param name="context">A context object that tells you what was requested.</param>
        /// <returns>response</returns>
        Task<TextContent> LoadContentAsync(ContentLoadingContext context);
    }   
}
