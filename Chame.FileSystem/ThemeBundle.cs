using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.FileSystem
{
    internal sealed class ThemeBundle
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
        public List<BundleFile> Css { get; set; } = new List<BundleFile>();

        /// <summary>
        /// JavaScript files
        /// </summary>
        [JsonProperty("js")]
        public List<BundleFile> Js { get; set; } = new List<BundleFile>();

        public sealed class BundleFile
        {
            /// <summary>
            /// File path
            /// </summary>
            [JsonProperty("path")]
            public string Path { get; set; }

            /// <summary>
            /// Filter (Regex)
            /// </summary>
            [JsonProperty("filter")]
            public string Filter { get; set; }
        }

    }
}
