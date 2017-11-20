using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame.Razor
{
    public class RazorPhysicalFileProviderOptions
    {
        /// <summary>
        /// The root directory. This should be an absolute path.
        /// </summary>
        public string Root { get; set; }

        public List<string> NamedControllers { get; set; } = new List<string>();
    }
}
