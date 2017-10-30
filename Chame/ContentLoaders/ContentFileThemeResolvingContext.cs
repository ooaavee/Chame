using Microsoft.AspNetCore.Http;

namespace Chame.ContentLoaders
{
    public class ContentFileThemeResolvingContext
    {
        public ContentFileThemeResolvingContext(HttpContext httpContext, IContentInfo contentInfo, string filter)
        {
            HttpContext = httpContext;
            ContentInfo = contentInfo;
            Filter = filter;
        }

        /// <summary>
        /// The current HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Defines what kind of content was requested.
        /// </summary>
        public IContentInfo ContentInfo { get; }

        /// <summary>
        /// Filter (optional).
        /// </summary>
        public string Filter { get; }
    }
}