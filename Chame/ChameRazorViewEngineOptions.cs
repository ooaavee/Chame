using System.Collections.Generic;
using System.Reflection;

namespace Chame
{
    public class ChameRazorViewEngineOptions
    {
        /// <summary>
        /// Assemblies where to look views that are embedded resources.
        /// </summary>
        public IList<Assembly> EmbeddedViewAssemblies { get; } = new List<Assembly>();
    }
}
