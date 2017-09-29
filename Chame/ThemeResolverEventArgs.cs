using Microsoft.AspNetCore.Http;

namespace Chame.Extensions
{
    public class ThemeResolverEventArgs
    {
        /// <summary>
        /// Http context
        /// </summary>
        public virtual HttpContext HttpContext { get; set; }

        /// <summary>
        /// What kind of content was requested
        /// </summary>
        public virtual ContentCategory Category { get; set; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public virtual string Filter { get; set; }
    }
}
