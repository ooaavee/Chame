using Chame.ContentLoaders;
using Chame.Razor;

namespace Chame
{
    public interface IThemeResolver
    {
        /// <summary>
        /// Resolves a theme that should be used when loading content files.
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>theme</returns>
        ITheme Resolve(ContentFileThemeResolvingContext context);

        /// <summary>
        /// Resolves a theme that should be used when loading Razor views.
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>theme</returns>
        ITheme Resolve(RazorThemeResolvingContext context);
    }
}
