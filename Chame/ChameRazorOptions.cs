using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Chame
{
    public class ChameRazorOptions
    {
        public ChameRazorOptions()
        {
        }

        /// <summary>
        /// Assemblies where to look views that are embedded resources.
        /// </summary>
        public IList<Assembly> EmbeddedViewAssemblies { get; } = new List<Assembly>();

    }
}
