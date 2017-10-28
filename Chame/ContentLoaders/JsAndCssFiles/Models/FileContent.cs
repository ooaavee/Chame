namespace Chame.ContentLoaders.JsAndCssFiles.Models
{
    internal class FileContent
    {
        /// <summary>
        /// Content bytes
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// HTTP ETag for content
        /// </summary>
        public string ETag { get; set; }
    }
}