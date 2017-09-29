using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.FileSystem
{
    internal sealed class Setup
    {
        /// <summary>
        /// All themes
        /// </summary>
        [JsonProperty("themes")]
        public List<ThemeBundle> Themes { get; set; } = new List<ThemeBundle>();
    }
}
