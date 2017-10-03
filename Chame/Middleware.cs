using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    internal sealed class Middleware
    {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, ChameContextFactory factory, ChameContextHandler handler)
        {
            ChameContext context;

            if (factory.TryCreateContext(httpContext, out context))
            {
                await handler.HandleAsync(context);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
