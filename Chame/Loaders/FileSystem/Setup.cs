using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.Loaders.FileSystem
{
    internal class Setup
    {
        /// <summary>
        /// All themes
        /// </summary>
        [JsonProperty("themes")]
        public List<ThemeBundle> Themes { get; set; } = new List<ThemeBundle>();
    }
}
