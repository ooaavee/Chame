using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.Models
{
    public class ContentFileTheme
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
        public virtual List<ContentFileThemeItem> CssFiles { get; set; } = new List<ContentFileThemeItem>();

        /// <summary>
        /// JavaScript files
        /// </summary>
        [JsonProperty("jsFiles")]
        public virtual List<ContentFileThemeItem> JsFiles { get; set; } = new List<ContentFileThemeItem>();
    }
}