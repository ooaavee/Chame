using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.Models
{
    public class ContentFileThemeContainer
    {
        /// <summary>
        /// All themes
        /// </summary>
        [JsonProperty("themes")]
        public virtual List<ContentFileTheme> Themes { get; set; } = new List<ContentFileTheme>();
    }
}
