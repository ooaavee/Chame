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
        /// 
        /// </summary>
        bool CanCombine { get; }
    }
}
