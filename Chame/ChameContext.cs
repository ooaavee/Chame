using Chame.Loaders;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameContext
    {
        public ChameContext(HttpContext httpContext, ContentCategory category, string filter, string eTag, IContentLoader[] loaders)
        {
            HttpContext = httpContext;
            Category = category;
            Filter = filter;
            ETag = eTag;
            Loaders = loaders;
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

        /// <summary>
        /// Content loaders
        /// </summary>
        public IContentLoader[] Loaders { get; }
    }
}
