using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chame
{
    public class ResponseContent
    {
        /// <summary>
        /// Response content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Encoding for response content
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// HTTP ETag for response content (optional)
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Merges multiple ResponseContent objects.
        /// </summary>
        public static ResponseContent Merge(IEnumerable<ResponseContent> items)
        {
            throw new NotImplementedException();

            return null;
        }

    }
}
