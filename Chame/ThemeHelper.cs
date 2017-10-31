using Chame.ContentLoaders;
using Chame.Razor;

namespace Chame
{
    internal static class ThemeHelper
    {
        internal static ITheme ResolveTheme(ContentFileThemeResolvingContext context, IThemeResolver resolver, ITheme fallbackTheme)
        {
            ITheme theme = null;
            if (resolver != null)
            {
                theme = resolver.Resolve(context);
            }
            return theme ?? fallbackTheme;
        }

        internal static ITheme ResolveTheme(RazorThemeResolvingContext context, IThemeResolver resolver, ITheme fallbackTheme)
        {
            ITheme theme = null;
            if (resolver != null)
            {
                theme = resolver.Resolve(context);
            }
            return theme ?? fallbackTheme;
        }
    }
}
