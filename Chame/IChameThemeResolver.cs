using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame
{
    public interface IChameThemeResolver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetTheme(ChameContentFileThemeResolveContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetTheme(ChameRazorThemeResolveContext context);
    }
}
