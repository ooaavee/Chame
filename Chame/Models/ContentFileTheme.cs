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
        /// CSS files for this theme
        /// </summary>
        [JsonProperty("cssFiles")]
        public virtual List<ContentFile> CssFiles { get; set; } = new List<ContentFile>();

        /// <summary>
        /// JavaScript files for this theme
        /// </summary>
        [JsonProperty("jsFiles")]
        public virtual List<ContentFile> JsFiles { get; set; } = new List<ContentFile>();
    }
}