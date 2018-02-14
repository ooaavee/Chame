using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.Internal
{
    internal class ChameUtility
    {
        private const string HttpContextItemsKey = "__Chame.ChameUtility.HttpContext.Items";

        private readonly ContentLoaderOptions _options;
        private readonly ILogger<ChameUtility> _logger;

        internal ChameUtility(IOptions<ContentLoaderOptions> options, ILogger<ChameUtility> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        internal ITheme GetTheme(HttpContext httpContext)
        {
            ITheme theme;

            if (httpContext.Items.TryGetValue(HttpContextItemsKey, out object v))
            {
                theme = (ITheme)v;
                return theme;
            }

            IThemeResolver resolver = httpContext.RequestServices.GetService<IThemeResolver>();

            if (resolver != null)
            {
                theme = resolver.GetTheme(httpContext);

                if (theme != null)
                {
                    httpContext.Items[HttpContextItemsKey] = theme;
                    return theme;
                }
            }

            theme = _options.DefaultTheme;

            if (theme != null)
            {
                httpContext.Items[HttpContextItemsKey] = theme;
                return theme;
            }

            _logger.LogCritical("Unable to resolve theme for the HttpContext.");

            throw new InvalidOperationException("Unable to resolve theme for the HttpContext.");
        }

        internal async Task<byte[]> GetContentAsync(string extension, string filter, ITheme theme, HttpContext httpContext)
        {
            var contentInfo = _options.ContentModel.GetByExtension(extension);
            if (contentInfo == null)
            {
                throw new InvalidOperationException($"'{extension}' is not supported extension, please check you IContentModel implementation.");
            }

            var context = new ContentLoadingContext(httpContext, contentInfo, theme, filter, null);
            var loaders = GetContentLoaders(httpContext, contentInfo);
            var responses = await LoadContentAsync(context, loaders);
            var response = Bundle(context, responses);
            return response.Status == ResponseStatus.Ok ? response.Data : null;
        }

        internal async Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, ITheme theme, HttpContext httpContext)
        {           
            var contentInfo = _options.ContentModel.GetByExtension(extension);
            if (contentInfo == null)
            {
                throw new InvalidOperationException($"'{extension}' is not supported extension, please check you IContentModel implementation.");
            }

            var context = new ContentLoadingContext(httpContext, contentInfo, theme, filter, null);
            var loaders = GetContentLoaders(httpContext, contentInfo);
            return await LoadContentAsync(context, loaders);
        }

        internal async Task<IList<ContentLoaderResponse>> LoadContentAsync(ContentLoadingContext context, List<IContentLoader> loaders)
        {
            var responses = new List<ContentLoaderResponse>();

            foreach (IContentLoader loader in loaders)
            {
                _logger.LogDebug(string.Format("Loading content by using '{0}' content loader.", loader.GetType().FullName));

                // load stuff
                ContentLoaderResponse response;
                try
                {
                    response = await loader.LoadContentAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Content loader '{0}' threw an unhandled exception.", loader.GetType().FullName);
                    throw;
                }

                // not found
                if (response == null || response.Status == ResponseStatus.NotFound)
                {
                    _logger.LogDebug(string.Format("Content loader '{0}' did not found any content.", loader.GetType().FullName));
                }

                // ok
                else if (response.Status == ResponseStatus.Ok)
                {
                    if (response.Data != null)
                    {
                        responses.Add(response);
                    }
                    else
                    {
                        _logger.LogWarning(string.Format("Content loader '{0}' retuned null content -> ignoring item.", loader.GetType().FullName));
                    }
                }

                // not modified
                else if (response.Status == ResponseStatus.NotModified)
                {
                    if (_options.SupportETag && !string.IsNullOrEmpty(context.ETag) && loaders.Count == 1)
                    {
                        responses.Add(response);
                    }
                    else
                    {
                        _logger.LogWarning(string.Format("Content loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseStatus.NotModified));
                    }
                }
            }

            // if not found -> invoke IContentNotFoundCallback if available
            if (!responses.Any())
            {
                IContentNotFoundCallback callback = context.HttpContext.RequestServices.GetService(typeof(IContentNotFoundCallback)) as IContentNotFoundCallback;

                if (callback != null)
                {
                    _logger.LogDebug($"Content not found - trying to get default content by using the {nameof(IContentNotFoundCallback)} service.");

                    byte[] data = await callback.GetDefaultContentAsync(context);

                    if (data == null)
                    {
                        _logger.LogDebug($"The {nameof(IContentNotFoundCallback)} service did not return the default content.");
                    }
                    else
                    {
                        _logger.LogDebug($"The {nameof(IContentNotFoundCallback)} service returned the default content.");

                        var file = new FileContent { Data = data };
                        var response = ContentLoaderResponse.Ok(file);
                        responses.Add(response);
                    }
                }
            }

            return responses;
        }

        internal List<IContentLoader> GetContentLoaders(HttpContext httpContext, IContentInfo contentInfo)
        {
            var loaders = new List<IContentLoader>();

            // get content loaders from request services and options
            foreach (var loader in httpContext.RequestServices.GetServices<IContentLoader>().Concat(_options.ContentLoaders))
            {
                if (loader.Supports().Any(supports => supports == contentInfo.Extension || supports == ContentLoaderOptions.ContentLoaderSupportsAll))
                {
                    loaders.Add(loader);
                }
                else
                {
                    _logger.LogDebug($"Ignoring content loader '{loader.GetType().FullName}' - it doesn't suppport '{contentInfo.Extension}'.");
                }
            }

            // sort content loaders if required
            if (loaders.Any())
            {
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
            }
            else
            {
                _logger.LogCritical("No content loaders found.");
            }

            return loaders;
        }

        internal ContentLoaderResponse Bundle(ContentLoadingContext context, IList<ContentLoaderResponse> responses)
        {
            if (responses.Count == 1)
            {
                return responses.First();
            }

            if (context.ContentInfo.AllowBundling)
            {
                var data = new List<byte>();
                foreach (var response in responses)
                {
                    if (response.Data != null)
                    {
                        data.AddRange(response.Data);
                    }
                }
                return ContentLoaderResponse.Ok(new FileContent {Data = data.ToArray()});
            }

            _logger.LogCritical($"Received multiple responses, but '{context.ContentInfo.MimeType}' content cannot be bundled. The first response will be used and others are ignored!");
            return responses.First();
        }

    }
}
