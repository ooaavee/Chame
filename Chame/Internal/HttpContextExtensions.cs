using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Chame.Internal
{
    internal static class HttpContextExtensions
    {
        public static IThemeResolver ThemeResolver(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService<IThemeResolver>();            
        }

        public static ChameUtility ChameUtility(this HttpContext httpContext)
        {
           return httpContext.RequestServices.GetRequiredService<ChameUtility>();            
        }

        public static IContentNotFoundCallback ContentNotFoundCallback(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService<IContentNotFoundCallback>();
        }

        public static IEnumerable<IContentLoader> ContentLoaders(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetServices<IContentLoader>();
        }

    }
}
