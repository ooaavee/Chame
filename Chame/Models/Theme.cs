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
        [JsonProperty("cssFiles")]
        public virtual List<ThemeFile> CssFiles { get; set; } = new List<ThemeFile>();

        /// <summary>
        /// JavaScript files
        /// </summary>
        [JsonProperty("jsFiles")]
        public virtual List<ThemeFile> JsFiles { get; set; } = new List<ThemeFile>();
    }
}