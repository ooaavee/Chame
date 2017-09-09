using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Chame.Loaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chame
{
    public class DefaultChameRequestHandler : IChameRequestHandler
    {
        private readonly ILogger<DefaultChameRequestHandler> _logger;

        public DefaultChameRequestHandler(ILogger<DefaultChameRequestHandler> logger)
        {
            _logger = logger;
        }

        public async Task<bool> HandleAsync(ChameContext context)
        {
            List<ResponseContent> responses = new List<ResponseContent>();

            foreach (IContentLoader loader in context.Loaders)
            {
                ResponseContent response = await loader.LoadAsync(context);
                if (response != null)
                {
                    responses.Add(response);
                }
            }

            if (responses.Any())
            {
                ResponseContent response = ResponseContent.Merge(responses);
                await WriteResponseAsync(context, response);
                return true;
            }

            return false;
        }

        private static async Task WriteResponseAsync(ChameContext context, ResponseContent response)
        {
            switch (context.Category)
            {
                case ContentCategory.Js:
                    context.HttpContext.Response.ContentType = "application/javascript";
                    break;
                case ContentCategory.Css:
                    context.HttpContext.Response.ContentType = "text/css";
                    break;
                default:
                    throw new InvalidOperationException("Unable to resolve content-type.");
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            await context.HttpContext.Response.WriteAsync(response.Content, response.Encoding);
        }

    }
}
