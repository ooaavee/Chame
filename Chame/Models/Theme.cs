using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.Models
{
    public class Theme
    {
        /// <summary>
        /// Theme name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Css files
        /// </summary>
        [JsonProperty("css")]
        public List<ThemeFile> Css { get; set; } = new List<ThemeFile>();

        /// <summary>
        /// JavaScript files
        /// </summary>
        [JsonProperty("js")]
        public List<ThemeFile> Js { get; set; } = new List<ThemeFile>();
    }
}