using Microsoft.AspNetCore.Http;

namespace Chame.ContentLoaders
{
    /// <summary>
    /// Encapsulates information about an individual content file loading request.
    /// </summary>
    public class ContentLoadingContext
    {
        public ContentLoadingContext(HttpContext httpContext, IContentInfo contentInfo, ITheme theme, string filter, string eTag)
        {
            HttpContext = httpContext;
            ContentInfo = contentInfo;
            Theme = theme;
            Filter = filter;
            ETag = eTag;
        }

        /// <summary>
        /// The current HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Content-info
        /// </summary>
        public IContentInfo ContentInfo { get; }

        /// <summary>
        /// Theme
        /// </summary>
        public ITheme Theme { get; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// HTTP ETag (optional)
        /// </summary>
        public string ETag { get; }
    }
}
