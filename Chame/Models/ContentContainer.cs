using System.Text;

namespace Chame.Models
{
    internal sealed class ContentContainer
    {
        public static Encoding DefaultEncoding = Encoding.UTF8;

        public ContentContainer(string content, string eTag)
        {
            Content = content;
            Encoding = DefaultEncoding;
            ETag = eTag;
        }

        public static ContentContainer Empty()
        {
            return new ContentContainer(string.Empty, null);
        }

        /// <summary>
        /// Response content
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Encoding for response content
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// HTTP ETag
        /// </summary>
        public string ETag { get; }
    }
}
