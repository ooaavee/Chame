﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chame.Models
{
    public class ThemeContainer
    {
        /// <summary>
        /// All themes
        /// </summary>
        [JsonProperty("themes")]
        public List<Theme> Themes { get; set; } = new List<Theme>();
    }
}