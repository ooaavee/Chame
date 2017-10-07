using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame
{
    public interface IChameThemeResolver
    {
        string GetTheme(ChameContentFileThemeResolveContext context);
        string GetTheme(ChameRazorThemeResolveContext context);
    }
}
