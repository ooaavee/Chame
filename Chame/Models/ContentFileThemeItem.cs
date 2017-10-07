using Newtonsoft.Json;

namespace Chame.Models
{
    public class ContentFileThemeItem
    {
        /// <summary>
        /// File path
        /// </summary>
        [JsonProperty("path")]
        public virtual string Path { get; set; }

        /// <summary>
        /// Filter (Regex)
        /// </summary>
        [JsonProperty("filter")]
        public virtual string Filter { get; set; }
    }
}