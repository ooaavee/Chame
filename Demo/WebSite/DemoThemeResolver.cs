using Chame;
using Chame.ContentLoaders;
using Chame.Razor;
using Microsoft.AspNetCore.Http;

namespace WebSite
{
    public class DemoThemeResolver : IThemeResolver
    {
        public ITheme Resolve(ContentFileThemeResolvingContext context)
        {
            return GetThemeFromHttpContext(context.HttpContext);
        }

        public ITheme Resolve(RazorThemeResolvingContext context)
        {
            return GetThemeFromHttpContext(context.HttpContext);
        }

        private static ITheme GetThemeFromHttpContext(HttpContext context)
        {
            string themeName;

            if (!context.User.Identity.IsAuthenticated)
            {
                themeName = "A";
            }
            else
            {
                if (context.User.Identity.Name == "themeB")
                {
                    themeName = "B";
                }
                else
                {
                    themeName = "C";
                }
            }

            ITheme theme = new DemoTheme(themeName);
            return theme;
        }

        private class DemoTheme : ITheme
        {
            private readonly string _name;

            public DemoTheme(string name)
            {
                _name = name;
            }

            public string GetName()
            {
                return _name;
            }
        }

    }
}