using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame.Internal
{
    internal sealed class ChameUtility
    {
        private const string HttpContextItemsKey = "__Chame.ChameUtility.HttpContext.Items";

        private readonly ContentLoaderOptions _options;
        private readonly ILogger<ChameUtility> _logger;

        public ChameUtility(IOptions<ContentLoaderOptions> options, ILogger<ChameUtility> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gets a theme that should be used with the specified HTTP context.
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <returns>theme</returns>
        public ITheme GetTheme(HttpContext httpContext)
        {
            // try to get "cached" theme
            if (httpContext.Items.TryGetValue(HttpContextItemsKey, out var cached))
            {
                return (ITheme) cached;
            }

            // invoke theme resolver and cache result for later usage
            IThemeResolver resolver = httpContext.FindThemeResolver();
            if (resolver != null)
            {
                ITheme theme = resolver.GetTheme(httpContext);
                if (theme != null)
                {
                    httpContext.Items[HttpContextItemsKey] = theme;
                    return theme;
                }
            }

            // try to get fallback theme cache result for later usage            
            if (_options.FallbackTheme != null)
            {
                httpContext.Items[HttpContextItemsKey] = _options.FallbackTheme;
                return _options.FallbackTheme;
            }

            _logger.LogCritical("Unable to resolve theme for the HttpContext.");
            throw new InvalidOperationException("Unable to resolve theme for the HttpContext.");
        }

        /// <summary>
        /// Loads content.
        /// </summary>
        public async Task<byte[]> LoadContentAsync(string extension, string filter, ITheme theme, HttpContext httpContext)
        {
            IContentInfo info = _options.ContentModel.GetByExtension(extension);
            if (info == null)
            {
                throw new InvalidOperationException($"'{extension}' is not supported extension, please check your {nameof(IContentModel)} implementation.");
            }

            List<IContentLoader> loaders = GetContentLoaders(httpContext, info);

            ContentLoadingContext context = ContentLoadingContext.Create(httpContext, info, theme, filter, null);

            List<ContentLoaderResponse> responses = await LoadContentAsync(context, loaders);

            if (responses.Any())
            {
                ContentLoaderResponse response = BundleResponses(context, responses);
                if (response.Status == ResponseStatus.Ok)
                {
                    return response.Data;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads content.
        /// </summary>
        public async Task<List<ContentLoaderResponse>> LoadContentAsync(ContentLoadingContext context, List<IContentLoader> loaders)
        {
            var responses = new List<ContentLoaderResponse>();

            foreach (IContentLoader loader in loaders)
            {
                _logger.LogDebug($"Loading content by using '{loader.GetType().FullName}' content loader.");

                // load stuff
                ContentLoaderResponse response;
                try
                {
                    response = await loader.LoadContentAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Content loader '{loader.GetType().FullName}' threw an unhandled exception.");
                    throw;
                }

                // not found
                if (response == null || response.Status == ResponseStatus.NotFound)
                {
                    _logger.LogDebug($"Content loader '{loader.GetType().FullName}' did not found any content.");
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
                        _logger.LogWarning($"Content loader '{loader.GetType().FullName}' retuned null content -> ignoring item.");
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
                        _logger.LogWarning($"Content loader '{loader.GetType().FullName}' returned status {ResponseStatus.NotModified}, which is wrong because ETags were not enabled.");
                    }
                }
            }

            // if not found -> invoke IContentNotFoundCallback if available
            if (!responses.Any())
            {
                IContentNotFoundCallback callback = context.HttpContext.FindContentNotFoundCallback();

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

        /// <summary>
        /// Gets a list of IContentLoader objects that should be used with the specified HttpContext when loading 
        /// content, defined by specified the IContentInfo object.
        /// </summary>
        public List<IContentLoader> GetContentLoaders(HttpContext httpContext, IContentInfo contentInfo)
        {
            var loaders = new List<IContentLoader>();

            // get content loaders from request services and options
            foreach (IContentLoader loader in httpContext.FindContentLoaders().Concat(_options.ContentLoaders))
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
                        _logger.LogWarning($"{nameof(IContentLoaderSorter)} implementation is not configured. Content loaders are invoked in arbitrary order.");
                    }
                }
            }
            else
            {
                _logger.LogCritical("No content loaders found.");
            }

            return loaders;
        }

        public ContentLoaderResponse BundleResponses(ContentLoadingContext context, IList<ContentLoaderResponse> responses)
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

                return ContentLoaderResponse.Ok(new FileContent { Data = data.ToArray() });
            }

            _logger.LogCritical($"Received multiple responses, but '{context.ContentInfo.MimeType}' content cannot be bundled. The first response will be used and others are ignored!");

            return responses.First();
        }

    }
}
