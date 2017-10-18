using System.Collections.Generic;
using System.Reflection;

namespace Chame
{
    public class RazorThemeOptions
    {
        public RazorThemeOptions()
        {
            ViewLocationTemplates.Add("Views/Themes/{0}/{{1}}/{{0}}.cshtml");
            ViewLocationTemplates.Add("Views/Themes/{0}/Shared/{{0}}.cshtml");
            ViewLocationTemplates.Add("Views/Themes/{0}/{{0}}.cshtml");
        }

        /// <summary>
        /// View location templates.
        /// </summary>
        public IList<string> ViewLocationTemplates { get; } = new List<string>();

        /// <summary>
        /// Assemblies where to look views that are embedded resources.
        /// </summary>
        public IList<Assembly> EmbeddedViewAssemblies { get; } = new List<Assembly>();
    }
}
