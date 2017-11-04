namespace Chame.ContentLoaders
{
    public class DefaultContentInfo : IContentInfo
    {
        public DefaultContentInfo(string mimeType, string extension, bool canCombine)
        {
            MimeType = mimeType;
            Extension = extension;
            CanCombine = canCombine;
        }

        public string MimeType { get; }
        public string Extension { get; }
        public bool CanCombine { get; }
    }
}