using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace Chame
{
    // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy

    public class ChameViewLocationExpander : IViewLocationExpander
    {
        private const string ThemeKey = "chame.razor.theme";

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Check that HTTP Context is available!
            if (context.ActionContext.HttpContext == null)
            {
                throw new InvalidOperationException("HTTP Context is not available.");
            }

            IOptions<ChameOptions> options =
                context.ActionContext.HttpContext.RequestServices
                    .GetService(typeof(IOptions<ChameOptions>)) as IOptions<ChameOptions>;


            if (options == null || options.Value == null)
            {
                // virhe
                throw new InvalidOperationException("");
            }

            IChameThemeResolver themeResolver = options.Value.ThemeResolver;
            if (themeResolver == null)
            {
                // virhe
                throw new InvalidOperationException("");

            }

            var resolveContext = new ChameRazorThemeResolveContext(
                context.ActionContext.HttpContext, 
                context.ViewName,
                context.ControllerName, 
                context.PageName, 
                context.AreaName, 
                context.IsMainPage, 
                context.Values);

            string theme = themeResolver.GetTheme(resolveContext);

            if (string.IsNullOrEmpty(theme))
            {
                throw new InvalidOperationException("Theme not found.");
            }

            context.Values[ThemeKey] = theme;

            //IChameRazorThemeResolver themeResolver = context.ActionContext.HttpContext.RequestServices.GetService(typeof(IChameRazorThemeResolver)) as IChameRazorThemeResolver;
            //if (themeResolver == null)
            //{
            //    throw new InvalidOperationException("IChameRazorThemeResolver implementation is not available.");
            //}

            //string theme = themeResolver.ResolveTheme(new ChameRazorThemeResolveContext
            //{
            //    HttpContext = context.ActionContext.HttpContext,
            //    AreaName = context.AreaName,
            //    ControllerName = context.ControllerName,
            //    IsMainPage = context.IsMainPage,
            //    PageName = context.PageName,
            //    Values = context.Values,
            //    ViewName = context.ViewName
            //});

            //if (string.IsNullOrEmpty(theme))
            //{
            //    throw new InvalidOperationException("Theme not found.");
            //}

            //context.Values[ThemeKey] = theme;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            string theme;
            
            if (context.Values.TryGetValue(ThemeKey, out theme))
            {
                IEnumerable<string> themeLocations = new[]
                {
                    // TODO: Olisi kiva jos tämä olisi konffattava :)
                    $"Views/Themes/{theme}/{{1}}/{{0}}.cshtml",
                    $"Views/Themes/{theme}/Shared/{{0}}.cshtml",
                    $"Views/Themes/{theme}/{{0}}.cshtml"
                };

                viewLocations = themeLocations.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}
