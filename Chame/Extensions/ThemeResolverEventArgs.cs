using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chame.Extensions
{
    public class ThemeResolverEventArgs
    {
        /// <summary>
        /// Http context
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// What kind of content was requested
        /// </summary>
        public ContentCategory Category { get; set; }

        /// <summary>
        /// Filter (optional)
        /// </summary>
        public string Filter { get; set; }
    }
}
