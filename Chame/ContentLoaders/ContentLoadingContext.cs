using Chame.Themes;
using Microsoft.AspNetCore.Http;

namespace Chame.ContentLoaders
{
    /// <summary>
    /// Encapsulates information about an individual content file loading request.
    /// </summary>
    public class ContentLoadingContext
    {
        internal static ContentLoadingContext Create(HttpContext httpContext, IContentInfo contentInfo, ITheme theme, string filter, string eTag)
        {
            return new ContentLoadingContext
            {
                HttpContext = httpContext,
                ContentInfo = contentInfo,
                Theme = theme,
                Filter = filter,
                ETag = eTag
            };
        }

        private ContentLoadingContext()
        {
        }

        /// <summary>
        /// The current HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; private set; }

        /// <summary>
        /// Content-info
        /// </summary>
        public IContentInfo ContentInfo { get; private set; }

        /// <summary>
        /// Theme
        /// </summary>
        public ITheme Theme { get; private set; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public string Filter { get; private set; }

        /// <summary>
        /// HTTP ETag (optional)
        /// </summary>
        public string ETag { get; private set; }
    }
}
