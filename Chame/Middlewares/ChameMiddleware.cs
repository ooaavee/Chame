﻿using System.Threading.Tasks;
using Chame.Services;
using Microsoft.AspNetCore.Http;

namespace Chame.Middlewares
{
    /// <summary>
    /// A middleware for handling Chame web-requests.
    /// </summary>
    internal sealed class ChameMiddleware
    {
        private readonly RequestDelegate _next;

        public ChameMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, ChameContextFactory factory, RequestProcessor processor)
        {
            if (factory.TryCreate(httpContext, out ChameContext context))
            {
                await processor.ProcessAsync(context);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
