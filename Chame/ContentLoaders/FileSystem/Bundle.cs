using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.ContentLoaders.FileSystem
{
    public class Bundle
    {
        public const string FileName = "bundle.json";

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        public class Group
        {
            [JsonProperty("filter")]
            public string Filter { get; set; }

            [JsonProperty("files")]
            public List<string> Files { get; set; }
        }

    }
}
