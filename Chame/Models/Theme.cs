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
        public virtual string Name { get; set; }

        /// <summary>
        /// Css files
        /// </summary>
        [JsonProperty("css")]
        public virtual List<ThemeFile> Css { get; set; } = new List<ThemeFile>();

        /// <summary>
        /// JavaScript files
        /// </summary>
        [JsonProperty("js")]
        public virtual List<ThemeFile> Js { get; set; } = new List<ThemeFile>();
    }
}