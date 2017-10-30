using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chame;
using Chame.ContentLoaders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace WebSite.Services
{
    public class DemoService
    {
        public const string ThemeA = "A";

        public const string ThemeB = "B";

        public async Task SwitchThemeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "demo_user") }, "demo");
                var principal = new ClaimsPrincipal(identity);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
        }
    }

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
            if (context.User.Identity.IsAuthenticated)
            {
                return new MyTheme(DemoService.ThemeB);
            }
            else
            {
                return new MyTheme(DemoService.ThemeA);
            }
        }

        private class MyTheme : ITheme
        {
            private readonly string _name;

            public MyTheme(string name)
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
