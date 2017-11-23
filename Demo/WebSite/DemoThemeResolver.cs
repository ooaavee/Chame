using Chame;
using Microsoft.AspNetCore.Http;

namespace WebSite
{
    public class DemoThemeResolver : IThemeResolver
    {
        public ITheme GetTheme(HttpContext httpContext)
        {
            string themeName;

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                themeName = "A";
            }
            else
            {
                if (httpContext.User.Identity.Name == "themeB")
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