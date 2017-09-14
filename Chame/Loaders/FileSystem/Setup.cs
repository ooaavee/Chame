using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.Loaders.FileSystem
{
    public sealed class Setup
    {
        /// <summary>
        /// All themes
        /// </summary>
        [JsonProperty("themes")]
        public List<ThemedBundle> Themes { get; set; } = new List<ThemedBundle>();

        public sealed class ThemedBundle 
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
            public List<string> Css { get; set; } = new List<string>();

            /// <summary>
            /// JavaScript files
            /// </summary>
            [JsonProperty("js")]
            public List<string> Js { get; set; } = new List<string>();

        }
    }
}
