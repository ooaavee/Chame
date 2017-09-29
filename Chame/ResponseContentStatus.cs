using System.Net;

namespace Chame
{
    public enum ResponseContentStatus
    {
        OK = HttpStatusCode.OK,
        NotFound = HttpStatusCode.NotFound,
        NotModified = HttpStatusCode.NotModified
    }
}
