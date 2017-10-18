using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    /// <summary>
    /// Encapsulates information about an individual Chame request.
    /// </summary>
    public class ContentLoadingContext
    {
        /// <summary>
        /// The current HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; internal set; }

        /// <summary>
        /// Defines what kind of content was requested.
        /// </summary>
        public ContentCategory Category { get; internal set; }

        /// <summary>
        /// Theme
        /// </summary>
        public string Theme { get; internal set; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public string Filter { get; internal set; }

        /// <summary>
        /// HTTP ETag (optional)
        /// </summary>
        public string ETag { get; internal set; }

        /// <summary>
        /// Content loaders
        /// </summary>
        public IReadOnlyCollection<IContentLoader> ContentLoaders { get; internal set; }
    }
}
