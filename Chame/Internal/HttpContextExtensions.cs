using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Chame.Internal
{
    internal static class HttpContextExtensions
    {
        public static IThemeResolver FindThemeResolver(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService<IThemeResolver>();            
        }

        public static ChameUtility FindChameUtility(this HttpContext httpContext)
        {
           return httpContext.RequestServices.GetRequiredService<ChameUtility>();            
        }

        public static IContentNotFoundCallback FindContentNotFoundCallback(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService<IContentNotFoundCallback>();
        }

        public static IEnumerable<IContentLoader> FindContentLoaders(this HttpContext httpContext)
        {
            return httpContext.RequestServices.GetServices<IContentLoader>();
        }

    }
}
