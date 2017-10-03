using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    /// <summary>
    /// Encapsulates information about an individual Chame request.
    /// </summary>
    public class ChameContext
    {
        public static ChameContext Create(HttpContext httpContext, ContentCategory category, string filter, string eTag, string theme, IReadOnlyCollection<IContentLoader> loaders)
        {
            if (httpContext == null)
            {
                throw  new ArgumentNullException(nameof(httpContext));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            if (loaders == null)
            {
                throw new ArgumentNullException(nameof(loaders));
            }

            if (!loaders.Any() || loaders.Any(x => x == null))
            {
                throw new ArgumentException("At least one loader must be specified.", nameof(loaders));
            }

            return new ChameContext
            {
                HttpContext = httpContext,
                Category = category,
                Filter = filter,
                ETag = eTag,
                Theme = theme,
                Loaders = loaders
            };
        }

        /// <summary>
        /// The current HTTP context.
        /// </summary>
        public virtual HttpContext HttpContext { get; internal set; }

        /// <summary>
        /// Defines what kind of content was requested.
        /// </summary>
        public virtual ContentCategory Category { get; internal set; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public virtual string Filter { get; internal set; }

        /// <summary>
        /// HTTP ETag (optional)
        /// </summary>
        public virtual string ETag { get; internal set; }

        /// <summary>
        /// Theme (optional)
        /// </summary>
        public virtual string Theme { get; internal set; }

        /// <summary>
        /// Content loaders
        /// </summary>
        public virtual IReadOnlyCollection<IContentLoader> Loaders { get; internal set; }
    }
}
