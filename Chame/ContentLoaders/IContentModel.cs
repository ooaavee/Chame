using System.Collections.Generic;

namespace Chame.ContentLoaders
{
    /// <summary>
    /// Defines supported content.
    /// </summary>
    public interface IContentModel
    {
        IReadOnlyCollection<IContentInfo> SupportedContent { get; }
    }
}
