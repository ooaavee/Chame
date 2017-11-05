namespace Chame.ContentLoaders
{
    public class FileContent
    {
        /// <summary>
        /// Content bytes
        /// </summary>
        public virtual byte[] Data { get; set; }

        /// <summary>
        /// HTTP ETag for content
        /// </summary>
        public virtual string ETag { get; set; }
    }
}