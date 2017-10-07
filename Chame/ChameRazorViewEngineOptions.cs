using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Chame
{
    public class ChameRazorViewEngineOptions
    {
        public ChameRazorViewEngineOptions()
        {
        }

        /// <summary>
        /// Assemblies where to look views that are embedded resources.
        /// </summary>
        public IList<Assembly> EmbeddedViewAssemblies { get; } = new List<Assembly>();

    }
}
