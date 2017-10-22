using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    /// <summary>
    /// Encapsulates information about an individual content file loading request.
    /// </summary>
    public class ContentLoadingContext
    {
        public ContentLoadingContext(HttpContext httpContext, ContentCategory category, string theme, string filter, string eTag, IReadOnlyCollection<IContentLoader> contentLoaders)
        {
            HttpContext = httpContext;
            Category = category;
            Theme = theme;
            Filter = filter;
            ETag = eTag;
            ContentLoaders = contentLoaders;
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

        /// <summary>
        /// Content loaders
        /// </summary>
        public IReadOnlyCollection<IContentLoader> ContentLoaders { get; }
    }
}
