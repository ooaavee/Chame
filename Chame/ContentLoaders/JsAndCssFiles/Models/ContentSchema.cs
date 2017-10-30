using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.ContentLoaders.JsAndCssFiles.Models
{
    public class ContentSchema
    {
        /// <summary>
        /// Common CSS files for all themes
        /// </summary>
        [JsonProperty("cssFiles")]
        public virtual List<ContentFile> CssFiles { get; set; } = new List<ContentFile>();

        /// <summary>
        /// Common JavaScript files for all themes
        /// </summary>
        [JsonProperty("jsFiles")]
        public virtual List<ContentFile> JsFiles { get; set; } = new List<ContentFile>();

        /// <summary>
        /// All themes
        /// </summary>
        [JsonProperty("themes")]
        public virtual List<ContentFileTheme> Themes { get; set; } = new List<ContentFileTheme>();

        public static ContentSchema Deserialize(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var schema = JsonConvert.DeserializeObject<ContentSchema>(content);
            if (schema == null)
            {
                throw new InvalidOperationException("Unable to deserialize JSON content.");
            }
            return schema;
        }
    }
}
