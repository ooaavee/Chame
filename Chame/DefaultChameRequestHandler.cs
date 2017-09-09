using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Chame.Loaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Chame
{
    public class DefaultChameRequestHandler : IChameRequestHandler
    {
        private readonly ChameOptions _options;
        private readonly ILogger<DefaultChameRequestHandler> _logger;

        public DefaultChameRequestHandler(ChameOptions options, ILogger<DefaultChameRequestHandler> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(ChameContext context)
        {
            // TODO: Loggins
            List<ResponseContent> responses = new List<ResponseContent>();

            foreach (IContentLoader loader in context.Loaders)
            {
                ResponseContent response = await loader.LoadAsync(context);
                if (response != null)
                {
                    responses.Add(response);
                }
            }

            int responseCount = responses.Count;

            if (responseCount > 0)
            {
                ResponseContent response = ResponseContent.Merge(responses);

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

                if (_options.ETagEnabled)
                {
                    if (responseCount == 1)
                    {
                        if (!string.IsNullOrEmpty(response.ETag))
                        {
                            context.HttpContext.Response.Headers.Add("ETag", new StringValues("response.ETag"));
                        }
                    }
                }

                await context.HttpContext.Response.WriteAsync(response.Content, response.Encoding);

                return true;
            }

            return false;
        }

    }
}
