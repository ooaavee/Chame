using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ContentFileThemeResolveContext
    {
        public ContentFileThemeResolveContext(HttpContext httpContext, ContentCategory category, string filter)
        {
            HttpContext = httpContext;
            Category = category;
            Filter = filter;
        }

        /// <summary>
        /// The current HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// What kind of content was requested.
        /// </summary>
        public ContentCategory Category { get; }

        /// <summary>
        /// Filter (optional).
        /// </summary>
        public string Filter { get; }
    }
}