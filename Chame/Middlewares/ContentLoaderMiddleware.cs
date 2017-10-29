using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Chame.Middlewares
{
    /// <summary>
    /// A middleware for loading JavaScript and CSS content.
    /// </summary>
    internal sealed class ContentLoaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ContentLoaderOptions _options;
        private readonly ILogger<ContentLoaderMiddleware> _logger;

        /// <summary>
        /// All valid request paths and corresponding content-types.
        /// </summary>
        private readonly IDictionary<string, IContentInfo> _paths = new Dictionary<string, IContentInfo>();

        public ContentLoaderMiddleware(RequestDelegate next, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger, string pathTemplate)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;

            // Register valid request paths for this middleware.
            foreach (IContentInfo content in _options.ContentModel.SupportedContent)
            {
                _paths[string.Format(pathTemplate, content.Extension).ToLower(CultureInfo.InvariantCulture)] = content;
            }
        }

        public async Task Invoke(HttpContext httpContext, IOptions<ContentLoaderOptions> options, ILogger<ContentLoaderMiddleware> logger)
        {
            IContentInfo content;

            if (IsValidRequest(httpContext, out content))
            {
                Tuple<ContentLoadingContext, IReadOnlyCollection<IContentLoader>> assets;

                if (TryPrepare(httpContext, content, out assets))
                {
                    ContentLoadingContext context = assets.Item1;
                    IReadOnlyCollection<IContentLoader> loaders = assets.Item2;

                    List<Content> loadedContent = await LoadContentAsync(context, loaders);

                    if (loadedContent.Any())
                    {
                        await WriteResponseAsync(context, loadedContent);
                        return;
                    }
                }
            }

            await _next(httpContext);
        }

        private bool IsValidRequest(HttpContext httpContext, out IContentInfo content)
        {
            content = null;

            // Must be HTTP GET.
            if (httpContext.Request.Method != HttpMethods.Get)
            {
                return false;
            }

            // Check that request path is valid.
            string path = httpContext.Request.Path.ToString().ToLower(CultureInfo.InvariantCulture);
            if (!_paths.TryGetValue(path, out content))
            {
                return false;
            }

            return true;
        }

        private bool TryPrepare(HttpContext httpContext, IContentInfo content, out Tuple<ContentLoadingContext, IReadOnlyCollection<IContentLoader>> assets)
        {
            assets = null;

            _logger.LogInformation(string.Format("Started to handle a HTTP request [path = {0}].", httpContext.Request.Path.ToString()));

            // parse an optional filter
            string filter = httpContext.Request.Query["filter"].FirstOrDefault();

            // get content loaders from request services and options
            List<IContentLoader> loaders = new List<IContentLoader>();
            foreach (IContentLoader loader in httpContext.RequestServices.GetServices<IContentLoader>().Concat(_options.ContentLoaders))
            {
                if (loader.ContentTypeExtensions().Any(x => x == content.Extension || x == "*"))
                {
                    loaders.Add(loader);
                }
            }

            if (loaders.Count == 0)
            {
                _logger.LogCritical("No content loaders found.");
                return false;
            }

            // sort content loaders
            if (loaders.Count > 1)
            {
                if (_options.ContentLoaderSorter != null)
                {
                    _options.ContentLoaderSorter.Sort(loaders);
                }
                else
                {
                    _logger.LogWarning(string.Format("{0} implementation is not configured. Content loaders are invoked in arbitrary order.", nameof(IContentLoaderSorter)));
                }
            }

            // HTTP ETag
            string eTag = null;
            if (_options.SupportETag)
            {
                if (httpContext.Request.Headers.ContainsKey("If-None-Match"))
                {
                    if (loaders.Count == 1)
                    {
                        eTag = httpContext.Request.Headers["If-None-Match"].FirstOrDefault();
                    }
                    else
                    {
                        _logger.LogDebug("HTTP ETag feature will be disabled for this request, because there is more than one content loader.");
                    }
                }
            }

            // resolve theme
            ITheme theme = ThemeResolver.Resolve(new ContentFileThemeResolvingContext(httpContext, content, filter), _options.ThemeResolver, _options.DefaultTheme);
            if (theme == null)
            {
                _logger.LogCritical("Could not resolve theme.");
                return false;
            }

            _logger.LogInformation(string.Format("A theme '{0}' will be used.", theme));

            ContentLoadingContext context = new ContentLoadingContext(httpContext, content, theme, filter, eTag);

            assets = new Tuple<ContentLoadingContext, IReadOnlyCollection<IContentLoader>>(context, loaders);

            return true;
        }

        private async Task WriteResponseAsync(ContentLoadingContext context, List<Content> loadedContent)
        {
            Content content;

            if (loadedContent.Count == 1)
            {
                content = loadedContent.First();
            }
            else
            {
                content = Content.Combine(loadedContent);
            }

            context.HttpContext.Response.ContentType = context.ContentInfo.MimeType;

            switch (content.Status)
            {
                case ResponseStatus.Ok:
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

                    if (_options.SupportETag && loadedContent.Count == 1 && !string.IsNullOrEmpty(content.ETag))
                    {
                        context.HttpContext.Response.Headers.Add("ETag", new StringValues(content.ETag));
                    }

                    await context.HttpContext.Response.Body.WriteAsync(content.Data, 0, content.Data.Length);
                    break;

                case ResponseStatus.NotModified:
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    break;
            }
        }

        private async Task<List<Content>> LoadContentAsync(ContentLoadingContext context, IReadOnlyCollection<IContentLoader> loaders)
        {
            var contents = new List<Content>();

            foreach (IContentLoader loader in loaders)
            {
                _logger.LogDebug(string.Format("Loading content by using '{0}' content loader.", loader.GetType().FullName));

                // load content from IContentLoader
                Content content;
                try
                {
                    content = await loader.LoadContentAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Content loader '{0}' threw an unhandled exception.", loader.GetType().FullName);
                    throw;
                }

                // not found
                if (content == null)
                {
                    _logger.LogDebug(string.Format("Content loader '{0}' did not found any content.", loader.GetType().FullName));
                    continue;
                }

                // handle loaded content status
                switch (content.Status)
                {
                    case ResponseStatus.NotFound:
                        _logger.LogDebug(string.Format("Content loader '{0}' did not found any content.", loader.GetType().FullName));
                        break;

                    case ResponseStatus.Ok:
                        if (content.Data != null)
                        {
                            contents.Add(content);
                        }
                        else
                        {
                            _logger.LogWarning(string.Format("Content loader '{0}' retuned null content -> ignoring item.", loader.GetType().FullName));
                        }
                        break;

                    case ResponseStatus.NotModified:
                        if (_options.SupportETag && !string.IsNullOrEmpty(context.ETag) && loaders.Count == 1)
                        {
                            contents.Add(content);
                        }
                        else
                        {
                            _logger.LogWarning(string.Format("Content loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseStatus.NotModified));
                        }
                        break;
                }
            }

            return contents;
        }
    }
}
