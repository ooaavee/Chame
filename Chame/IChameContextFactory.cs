using Microsoft.AspNetCore.Http;

namespace Chame
{
    public interface IChameContextFactory
    {
        bool TryCreateContext(HttpContext httpContext, out ChameContext context);
    }
}
