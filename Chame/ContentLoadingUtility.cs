using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame
{
    public static class ContentLoadingUtility
    {
        public static async Task<byte[]> GetContentAsync(string extension, string filter, string themeName, HttpContext httpContext)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (themeName == null)
            {
                throw new ArgumentNullException(nameof(themeName));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var options = GetOptions(httpContext);

            var contentInfo = options.ContentModel.GetByExtension(extension);
            if (contentInfo == null)
            {
                throw new InvalidOperationException($"'{extension}' is not supported extension, please check you IContentModel implementation.");
            }

            var theme = new AdHocTheme(themeName);
            var context = new ContentLoadingContext(httpContext, contentInfo, theme, filter, null);
            var loaders = GetContentLoaders(httpContext, contentInfo);
            var responses = await LoadContentAsync(context, loaders);
            var response = Bundle(context, responses);
            return response.Status == ResponseStatus.Ok ? response.Data : null;
        }

        public static async Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, string themeName, HttpContext httpContext)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (themeName == null)
            {
                throw new ArgumentNullException(nameof(themeName));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var options = GetOptions(httpContext);

            var contentInfo = options.ContentModel.GetByExtension(extension);
            if (contentInfo == null)
            {
                throw new InvalidOperationException($"'{extension}' is not supported extension, please check you IContentModel implementation.");
            }

            var theme = new AdHocTheme(themeName);
            var context = new ContentLoadingContext(httpContext, contentInfo, theme, filter, null);
            var loaders = GetContentLoaders(httpContext, contentInfo);
            return await LoadContentAsync(context, loaders);
        }

        internal static async Task<IList<ContentLoaderResponse>> LoadContentAsync(ContentLoadingContext context, List<IContentLoader> loaders)
        {
            var options = GetOptions(context.HttpContext);
            var logger = GetLogger(context.HttpContext);

            var responses = new List<ContentLoaderResponse>();

            foreach (IContentLoader loader in loaders)
            {
                logger.LogDebug(string.Format("Loading content by using '{0}' content loader.", loader.GetType().FullName));

                // load stuff
                ContentLoaderResponse response;
                try
                {
                    response = await loader.LoadContentAsync(context);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Content loader '{0}' threw an unhandled exception.", loader.GetType().FullName);
                    throw;
                }

                // not found
                if (response == null || response.Status == ResponseStatus.NotFound)
                {
                    logger.LogDebug(string.Format("Content loader '{0}' did not found any content.", loader.GetType().FullName));
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
                        logger.LogWarning(string.Format("Content loader '{0}' retuned null content -> ignoring item.", loader.GetType().FullName));
                    }
                }

                // not modified
                else if (response.Status == ResponseStatus.NotModified)
                {
                    if (options.SupportETag && !string.IsNullOrEmpty(context.ETag) && loaders.Count == 1)
                    {
                        responses.Add(response);
                    }
                    else
                    {
                        logger.LogWarning(string.Format("Content loader '{0}' returned status {1}, which is wrong because ETags were not enabled.", loader.GetType().FullName, ResponseStatus.NotModified));
                    }
                }
            }

            // if not found -> invoke IContentNotFoundCallback if available
            if (!responses.Any())
            {
                IContentNotFoundCallback callback = context.HttpContext.RequestServices.GetService(typeof(IContentNotFoundCallback)) as IContentNotFoundCallback;

                if (callback != null)
                {
                    logger.LogDebug($"Content not found - trying to get default content by using the {nameof(IContentNotFoundCallback)} service.");

                    byte[] data = await callback.GetDefaultContentAsync(context);

                    if (data == null)
                    {
                        logger.LogDebug($"The {nameof(IContentNotFoundCallback)} service did not return the default content.");
                    }
                    else
                    {
                        logger.LogDebug($"The {nameof(IContentNotFoundCallback)} service returned the default content.");

                        var file = new FileContent { Data = data };
                        var response = ContentLoaderResponse.Ok(file);
                        responses.Add(response);
                    }
                }
            }

            return responses;
        }

        internal static List<IContentLoader> GetContentLoaders(HttpContext httpContext, IContentInfo contentInfo)
        {
            var options = GetOptions(httpContext);
            var logger = GetLogger(httpContext);

            var loaders = new List<IContentLoader>();

            // get content loaders from request services and options
            foreach (var loader in httpContext.RequestServices.GetServices<IContentLoader>().Concat(options.ContentLoaders))
            {
                if (loader.Supports().Any(supports => supports == contentInfo.Extension || supports == ContentLoaderOptions.ContentLoaderSupportsAll))
                {
                    loaders.Add(loader);
                }
                else
                {
                    logger.LogDebug($"Ignoring content loader '{loader.GetType().FullName}' - it doesn't suppport '{contentInfo.Extension}'.");
                }
            }

            // sort content loaders if required
            if (loaders.Any())
            {
                if (loaders.Count > 1)
                {
                    if (options.ContentLoaderSorter != null)
                    {
                        options.ContentLoaderSorter.Sort(loaders);
                    }
                    else
                    {
                        logger.LogWarning(string.Format("{0} implementation is not configured. Content loaders are invoked in arbitrary order.", nameof(IContentLoaderSorter)));
                    }
                }
            }
            else
            {
                logger.LogCritical("No content loaders found.");
            }

            return loaders;
        }

        internal static ContentLoaderResponse Bundle(ContentLoadingContext context, IList<ContentLoaderResponse> responses)
        {
            var logger = GetLogger(context.HttpContext);

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

            logger.LogCritical($"Received multiple responses, but '{context.ContentInfo.MimeType}' content cannot be bundled. The first response will be used and others are ignored!");
            return responses.First();
        }

        private static ContentLoaderOptions GetOptions(HttpContext httpContext)
        {
            return httpContext.RequestServices.GetRequiredService<IOptions<ContentLoaderOptions>>().Value;
        }

        private static ILogger GetLogger(HttpContext httpContext)
        {
            return httpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(ContentLoadingUtility));
        }
    }
}
