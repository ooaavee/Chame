using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chame.Caching;
using Chame.ContentLoaders.JsAndCssFiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.ContentLoaders.FileSystem
{
    public class FileSystemLoader : IContentLoader
    {
        private readonly ContentLoaderOptions _options1;
        private readonly FileSystemLoaderOptions _options2;
        private readonly ContentCache _cache;
        private readonly bool _useCache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<FileSystemLoader> _logger;
        private readonly PhysicalFileProvider _provider;

        public FileSystemLoader(IOptions<ContentLoaderOptions> options1, IOptions<FileSystemLoaderOptions> options2, ContentCache cache, IHostingEnvironment env, ILogger<FileSystemLoader> logger)
        {
            if (options1 == null)
            {
                throw new ArgumentNullException(nameof(options1));
            }

            if (options2 == null)
            {
                throw new ArgumentNullException(nameof(options2));
            }

            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _options1 = options1.Value;
            _options2 = options2.Value;
            _cache = cache;
            _useCache = options2.Value.Caching.IsEnabled(env);
            _env = env;
            _logger = logger;
            _provider = new PhysicalFileProvider(_options2.Root);
        }

        /// <summary>
        /// Content loader priority. 
        /// An execution order of content loaders are sorted by this property. 
        /// This is only meaningful if there are more than one content loaders!
        /// </summary>
        public double Priority => 1;

        /// <summary>
        /// Content-type extensions that are supported by the content loader.
        /// </summary>
        /// <returns>supported content-type extensions</returns>
        public IEnumerable<string> Supports()
        {
            yield return ContentLoaderOptions.ContentLoaderSupportsAll;
        }

        /// <summary>
        /// Loads content.
        /// </summary>
        /// <param name="context">A context object that tells you what was requested.</param>
        /// <returns>loaded content</returns>
        public Task<ContentLoaderResponse> LoadContentAsync(ContentLoadingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(Load(context));
        }

      

        private ContentLoaderResponse Load(ContentLoadingContext context)
        {
            FileContent content;

            // first try to use cached content
            if (_useCache)
            {
                content = _cache.Get<FileContent>(context);
                if (content != null)
                {
                    return ContentLoaderResponse.CreateResponse(content, context, _options1);
                }
            }

            Bundle bundle;
            if (context.ContentInfo.AllowBundling && TryGetBundle(context, out bundle))
            {
                content = LoadBundle(bundle, context);
            }
            else
            {
                content = LoadPlain(context);
            }

            return ContentLoaderResponse.CreateResponse(content, context, _options1);
        }

        private bool TryGetBundle(ContentLoadingContext context, out Bundle bundle)
        {
            bundle = null;

            // TODO: try to load bundle

            return false;
        }

        // lataa bundlesta filterin perusteella (jos ei ole filteriä, niin error!)
        private FileContent LoadBundle(Bundle bundle, ContentLoadingContext context)
        {
            return null;
        }

        // lataa ilman bundlea käyttäällä filteriä (jos ei ole filteriä, niin error!)
        private FileContent LoadPlain(ContentLoadingContext context)
        {
            return null;
        }


        private static string PathFor(ITheme theme, IContentInfo contentInfo, string fileName)
        {
            string path = $"/{theme.GetName()}/{contentInfo.Extension}/{fileName}";
            return path;
        }

    }
}
