using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame
{
    public class ChameRazorThemeResolverOptions
    {
        /// <summary>
        /// The name of the default theme that should be used, if <see cref="ResolveTheme"/> is not available.
        /// </summary>
        public string DefaultTheme { get; set; }

        /// <summary>
        /// A function for resolving a theme to be used when loading Razor views.
        /// </summary>
        public Func<ChameRazorThemeResolveContext, string> ResolveTheme { get; set; }

    }
}
