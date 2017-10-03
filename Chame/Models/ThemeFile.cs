using Newtonsoft.Json;

namespace Chame.Models
{
    public class ThemeFile
    {
        /// <summary>
        /// File path
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// Filter (Regex)
        /// </summary>
        [JsonProperty("filter")]
        public string Filter { get; set; }
    }
}