using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameContentFileThemeResolveContext
    {
        public ChameContentFileThemeResolveContext(HttpContext httpContext, ContentCategory category, string filter)
        {
            HttpContext = httpContext;
            Category = category;
            Filter = filter;
        }

        /// <summary>
        /// Http context
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// What kind of content was requested
        /// </summary>
        public ContentCategory Category { get; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public string Filter { get; }
    }
}