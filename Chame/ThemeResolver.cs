using Chame.ContentLoaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Chame
{
    internal static class ThemeResolver
    {
        internal static ITheme Resolve(ContentFileThemeResolvingContext context, IThemeResolver resolver, ITheme fallbackTheme)
        {
            ITheme theme = null;

            if (resolver != null)
            {
                theme = resolver.Resolve(context);
            }

            if (theme == null)
            {
                theme = fallbackTheme;
            }

            return theme;
        }

        internal static ITheme Resolve(RazorThemeResolvingContext context, IThemeResolver resolver, ITheme fallbackTheme)
        {
            ITheme theme = null;

            if (resolver != null)
            {
                theme = resolver.Resolve(context);
            }

            if (theme == null)
            {
                theme = fallbackTheme;
            }

            return theme;
        }

    }
}
