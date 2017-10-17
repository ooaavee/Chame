﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chame;
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

    public class DemoThemeResolver : IChameThemeResolver
    {
        public string GetTheme(ChameContentFileThemeResolveContext context)
        {
            return GetThemeFromHttpContext(context.HttpContext);
        }

        public string GetTheme(ChameRazorThemeResolveContext context)
        {
            return GetThemeFromHttpContext(context.HttpContext);
        }

        private static string GetThemeFromHttpContext(HttpContext context)
        {
            return context.User.Identity.IsAuthenticated ?
                DemoService.ThemeB :
                DemoService.ThemeA;
        }

    }

}