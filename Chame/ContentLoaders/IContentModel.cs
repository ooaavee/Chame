using System.Collections.Generic;

namespace Chame.ContentLoaders
{
    /// <summary>
    /// Defines supported content.
    /// </summary>
    public interface IContentModel
    {
        /// <summary>
        /// Supported content
        /// </summary>
        IList<IContentInfo> SupportedContent { get; }

        IContentInfo GetByExtension(string extension);
    }
}
