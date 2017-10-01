using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Chame.Services
{
    internal sealed class RequestHandler : IChameRequestHandler
    {
        private readonly ChameOptions _options;
        private readonly ILogger<RequestHandler> _logger;

        public RequestHandler(IOptions<ChameOptions> options, ILogger<RequestHandler> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task HandleAsync(ChameContext context)
        {
            var responses = new List<ResponseContent>();

            foreach (IContentLoader loader in context.Loaders)
            {
                _logger.LogDebug(string.Format("Loading content by using '{0}' loader.", loader.GetType().FullName));

                // Load content
                ResponseContent response;
                try
                {
                    response = await loader.LoadAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Loader '{0}' threw an exception {1}.", loader.GetType().FullName, ex);
                    throw;
                }

                if (response == null || response.Status == ResponseContentStatus.NotFound)
                {
                    _logger.LogDebug(string.Format("Loader '{0}' did not found any content.", loader.GetType().FullName));
                }
                else if (response.Status == ResponseContentStatus.OK)
                {
                    // Validate loaded content
                    bool valid = true;
                    if (response.Content == null)
                    {
                        valid = false;
                        _logger.LogWarning(string.Format("Loader '{0}' retuned null content, ignoring item.", loader.GetType().FullName));
                    }
                    if (response.Encoding == null)
                    {
                        valid = false;
                        _logger.LogWarning(string.Format("Loader '{0}' retuned null encoding, ignoring item.", loader.GetType().FullName));
                    }

                    // Use loaded content
                    if (valid)
                    {
                        responses.Add(response);
                    }
                }
                else if (response.Status == ResponseContentStatus.NotModified)
                {
                    if (_options.UseETag && !string.IsNullOrEmpty(context.ETag) && context.Loaders.Count == 1)
                    {
                        responses.Add(response);
                    }
                    else
                    {
                        _logger.LogWarning(string.Format("Loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseContentStatus.NotModified));
                    }
                }
            }

            int reponseCount = responses.Count;

            if (reponseCount == 0)
            {
                _logger.LogDebug("None of loader(s) found any content.");

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else if (reponseCount > 0)
            {
                ResponseContent response = reponseCount > 1 ? Merge(responses) : responses.First();

                switch (context.Category)
                {
                    case ContentCategory.Js:
                        context.HttpContext.Response.ContentType = "application/javascript";
                        break;

                    case ContentCategory.Css:
                        context.HttpContext.Response.ContentType = "text/css";
                        break;

                    default:
                        _logger.LogError(string.Format("Invalid content category '{0}'.", context.Category));
                        throw new InvalidOperationException(string.Format("Invalid content category '{0}'.", context.Category));
                }

                switch (response.Status)
                {
                    case ResponseContentStatus.OK:
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

                        if (_options.UseETag && responses.Count == 1 && !string.IsNullOrEmpty(response.ETag))
                        {
                            context.HttpContext.Response.Headers.Add("ETag", new StringValues(response.ETag));
                        }

                        await context.HttpContext.Response.WriteAsync(response.Content, response.Encoding);
                        break;

                    case ResponseContentStatus.NotModified:
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                        break;

                    case ResponseContentStatus.NotFound:
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    default:
                        _logger.LogError(string.Format("Invalid reponse status '{0}'.", response.Status));
                        throw new InvalidOperationException(string.Format("Invalid reponse status '{0}'.", response.Status));
                }

            }
        }

        /// <summary>
        /// Merges multiple ResponseContent objects.
        /// </summary>
        private ResponseContent Merge(IEnumerable<ResponseContent> items)
        {
            _logger.LogDebug("Merging content from multiple loaders.");

            var response = new ResponseContent
            {
                Encoding = null,
                ETag = null,
                Status = ResponseContentStatus.OK
            };

            var buf = new StringBuilder();

            foreach (var item in items)
            {
                if (response.Encoding != null && response.Encoding.EncodingName != item.Encoding.EncodingName)
                {
                    var message = string.Format("Failed to merge ResponseContent object because multiple encondings were used ({0} and {1}).", response.Encoding.EncodingName, item.Encoding.EncodingName);
                    _logger.LogError(message);
                    throw new InvalidOperationException(message);
                }

                buf.Append(item.Content);

                response.Encoding = item.Encoding;
            }

            response.Content = buf.ToString();

            return response;
        }

    }
}
