using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public class ChameMiddleware
    {
        private readonly RequestDelegate _next;

        public ChameMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IChameContextFactory contextFactory, IChameRequestHandler requestHandler)
        {
            ChameContext context;

            if (contextFactory.TryCreateContext(httpContext, out context))
            {
                if (await requestHandler.HandleAsync(context))
                {
                    return;
                }
            }

            await _next(httpContext);
        }
    }
}
