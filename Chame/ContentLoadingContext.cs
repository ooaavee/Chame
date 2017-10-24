using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    /// <summary>
    /// Encapsulates information about an individual content file loading request.
    /// </summary>
    public class ContentLoadingContext
    {
        public ContentLoadingContext(HttpContext httpContext, IContentInfo contentInfo, string theme, string filter, string eTag)
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

        public IContentInfo ContentInfo { get; }

        /// <summary>
        /// Theme
        /// </summary>
        public string Theme { get; }

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
