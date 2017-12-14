using System;
using System.Collections.Generic;
using System.Text;
using Chame.ContentLoaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Chame
{
    public static class ContentLoadingUtility
    {

        internal static List<IContentLoader> GetContentLoaders(HttpContext http, IContentInfo info)
        {
            ILoggerFactory loggerFactory = http.RequestServices.GetRequiredService<ILoggerFactory>();
            ILogger _logger = loggerFactory.CreateLogger(typeof(ContentLoadingUtility));

            IOptions<ContentLoaderOptions> options = http.RequestServices.GetRequiredService<IOptions<ContentLoaderOptions>>();
            //IList<IContentLoader> _contentLoaders = options.Value.ContentLoaders;
            //IContentLoaderSorter _contentLoaderSorter = options.Value.ContentLoaderSorter;


            // get content loaders from request services and options
            var loaders = new List<IContentLoader>();
            foreach (var loader in http.RequestServices.GetServices<IContentLoader>().Concat(options.Value.ContentLoaders))
            {
                if (loader.Supports().Any(supports => supports == info.Extension || supports == ContentLoaderOptions.ContentLoaderSupportsAll))
                {
                    loaders.Add(loader);
                }
                else
                {
                    _logger.LogDebug($"Ignoring content loader '{loader.GetType().FullName}' - it doesn't suppport '{info.Extension}'.");
                }
            }

            if (loaders.Any())
            {
                // sort content loaders if required
                if (loaders.Count > 1)
                {
                    if (options.Value.ContentLoaderSorter != null)
                    {
                        options.Value.ContentLoaderSorter.Sort(loaders);
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


    }
}
