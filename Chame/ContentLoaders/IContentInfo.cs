namespace Chame.ContentLoaders
{
    public interface IContentInfo
    {
        /// <summary>
        /// MIME Type
        /// </summary>
        string MimeType { get; }

        /// <summary>
        /// File extension for the MIME type
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Allow bundling for this content?
        /// 
        /// Example:
        /// JavaScript and CSS files can be bundled - you known ..but you cannot bundle multiple JPGs.
        /// </summary>
        bool AllowBundling { get; }
    }
}
