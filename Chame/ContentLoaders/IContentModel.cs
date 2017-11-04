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
        IReadOnlyCollection<IContentInfo> SupportedContent { get; }
    }
}
