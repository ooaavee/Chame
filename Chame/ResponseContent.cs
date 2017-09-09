using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chame
{
    public class ResponseContent
    {
        public string Content { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public static ResponseContent Merge(IEnumerable<ResponseContent> items)
        {
            return null;
        }

    }
}
