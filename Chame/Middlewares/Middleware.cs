using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chame.Middlewares
{
    internal sealed class Middleware
    {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IChameContextFactory contextFactory, IChameRequestHandler requestHandler)
        {
            ChameContext context;

            if (contextFactory.TryCreateContext(httpContext, out context))
            {
                await requestHandler.HandleAsync(context);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
