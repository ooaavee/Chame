using Newtonsoft.Json;

namespace Chame.ContentLoaders.JsAndCssFiles.Models
{
    public class ContentFile
    {
        /// <summary>
        /// File path
        /// </summary>
        [JsonProperty("path")]
        public virtual string Path { get; set; }

        /// <summary>
        /// Filter
        /// </summary>
        [JsonProperty("filter")]
        public virtual string Filter { get; set; }
    }
}