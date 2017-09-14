using Chame.Loaders;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    /// <summary>
    /// 
    /// </summary>
    public class ChameContext
    {
        public ChameContext(HttpContext httpContext, ContentCategory category, string filter, string eTag, string theme, IContentLoader[] loaders)
        {
            HttpContext = httpContext;
            Category = category;
            Filter = filter;
            ETag = eTag;
            Loaders = loaders;
            Theme = theme;
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

        /// <summary>
        /// HTTP ETag (optional)
        /// </summary>
        public string ETag { get; }

        public string Theme { get; }

        /// <summary>
        /// Content loaders
        /// </summary>
        public IContentLoader[] Loaders { get; }
    }
}
