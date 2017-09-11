using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Chame.Loaders;
using Microsoft.AspNetCore.Http;
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

        public async Task HandleAsync(ChameContext context)
        {
            List<ResponseContent> responses = new List<ResponseContent>();

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
                else if (response.Status == ResponseContentStatus.Ok)
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
                    if (_options.UseETag && !string.IsNullOrEmpty(context.ETag) && context.Loaders.Length == 1)
                    {
                        responses.Add(response);
                    }
                    else
                    {
                        _logger.LogWarning(string.Format("Loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseContentStatus.NotModified));
                    }
                }
            }

            int responseCount = responses.Count;

            if (responseCount == 0)
            {
                _logger.LogDebug("None of loader(s) found any content.");

                // HTTP Status Code
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Reponse content
                await context.HttpContext.Response.WriteAsync("", ResponseContent.DefaultEncoding);
            }
            else if (responseCount > 0)
            {
                ResponseContent response = responseCount > 1 ? Merge(responses) : responses.First();

                // Content-Type
                string contentType;
                switch (context.Category)
                {
                    case ContentCategory.Js:
                        contentType = "application/javascript";
                        break;
                    case ContentCategory.Css:
                        contentType = "text/css";
                        break;
                    default:
                        string error = "Unable to resolve content-type.";
                        _logger.LogError(error);
                        throw new InvalidOperationException(error);
                }
                context.HttpContext.Response.ContentType = contentType;

                // HTTP ETag
                if (_options.UseETag && response.Status == ResponseContentStatus.Ok && responseCount == 1 &&
                    !string.IsNullOrEmpty(response.ETag))
                {
                    context.HttpContext.Response.Headers.Add("ETag", new StringValues(response.ETag));
                }

                // HTTP Status Code
                HttpStatusCode statusCode;
                switch (response.Status)
                {
                    case ResponseContentStatus.Ok:
                        statusCode = HttpStatusCode.OK;
                        break;
                    case ResponseContentStatus.NotModified:
                        statusCode = HttpStatusCode.NotModified;
                        break;
                    default:
                        statusCode = HttpStatusCode.NotFound;
                        break;
                }
                context.HttpContext.Response.StatusCode = (int) statusCode;

                // Reponse content
                string content = response.Status == ResponseContentStatus.Ok ? response.Content : "";
                Encoding encoding = response.Status == ResponseContentStatus.Ok ? response.Encoding : ResponseContent.DefaultEncoding;
                await context.HttpContext.Response.WriteAsync(content, encoding);
            }
        }

        /// <summary>
        /// Merges multiple ResponseContent objects.
        /// </summary>
        private ResponseContent Merge(IEnumerable<ResponseContent> items)
        {
            _logger.LogDebug("Merging content from multiple loaders.");

            ResponseContent response = new ResponseContent
            {
                Encoding = null,
                ETag = null,
                Status = ResponseContentStatus.Ok
            };

            StringBuilder buffer = new StringBuilder();

            foreach (ResponseContent item in items)
            {
                if (response.Encoding != null)
                {
                    if (response.Encoding.EncodingName != item.Encoding.EncodingName)
                    {
                        string error = string.Format("Failed to merge ResponseContent object because multiple encondings were used ({0} and {1}).", response.Encoding.EncodingName, item.Encoding.EncodingName);
                        _logger.LogError(error);
                        throw new InvalidOperationException(error);
                    }
                }

                buffer.Append(item.Content);

                response.Encoding = item.Encoding;
            }

            response.Content = buffer.ToString();

            return response;
        }

    }
}
