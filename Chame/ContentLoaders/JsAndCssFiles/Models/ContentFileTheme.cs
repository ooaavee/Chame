using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.ContentLoaders.JsAndCssFiles.Models
{
    public class ContentFileTheme
    {
        /// <summary>
        /// Theme id
        /// </summary>
        [JsonProperty("id")]
        public virtual string Id { get; set; }

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