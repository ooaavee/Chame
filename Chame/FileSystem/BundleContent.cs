using System.Text;

namespace Chame.FileSystem
{
    internal sealed class BundleContent
    {
        /// <summary>
        /// Response content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Encoding for response content
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// HTTP ETag
        /// </summary>
        public string ETag { get; set; }
    }
}
