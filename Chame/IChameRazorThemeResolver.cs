using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public interface IChameRazorThemeResolver
    {
        /// <summary>
        /// Resolves a theme that should be used.
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>theme name</returns>
        string ResolveTheme(ChameRazorThemeResolveContext context);
    }
}
