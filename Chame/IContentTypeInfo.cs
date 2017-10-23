using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame
{
    public interface IContentTypeInfo
    {
        string MimeType { get; }
        string Code { get; }
        bool CanCombine { get; }
    }

    public class DefaultContentTypeInfo : IContentTypeInfo
    {
        public string MimeType { get; set; }
        public string Code { get; set; }
        public bool CanCombine { get; set; }

    }

}
