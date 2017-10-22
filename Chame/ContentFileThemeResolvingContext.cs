using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ContentFileThemeResolvingContext
    {
        public ContentFileThemeResolvingContext(HttpContext httpContext, ContentCategory category, string filter)
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
        /// Defines what kind of content was requested.
        /// </summary>
        public ContentCategory Category { get; }

        /// <summary>
        /// Filter (optional).
        /// </summary>
        public string Filter { get; }
    }
}