using Microsoft.AspNetCore.Http;

namespace Chame.Themes
{    
    public interface IThemeResolver
    {
        /// <summary>
        /// Resolves a theme that should be used with the specified HTTP context.
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <returns>theme</returns>
        ITheme GetTheme(HttpContext httpContext);
    }
}
