using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Chame.Caching;
using Chame.ContentLoaders.JsAndCssFiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Chame.ContentLoaders.FileSystem
{
    public class FileSystemLoader : IContentLoader
    {
        private readonly ContentLoaderOptions _options1;
        private readonly FileSystemLoaderOptions _options2;
        private readonly ContentCache _cache;
        private readonly ILogger<FileSystemLoader> _logger;
        private readonly PhysicalFileProvider _provider;

        public FileSystemLoader(IOptions<ContentLoaderOptions> options1, IOptions<FileSystemLoaderOptions> options2, ContentCache cache, ILogger<FileSystemLoader> logger)
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

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _options1 = options1.Value;
            _options2 = options2.Value;
            _cache = cache;
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
            // first try to use cached content
            FileContent content = _cache.Get<FileContent>(_options2.Caching, context);
            if (content != null)
            {
                return ContentLoaderResponse.CreateResponse(content, context, _options1);
            }

            if (string.IsNullOrEmpty(context.Filter))
            { 
                _logger.LogInformation($"Unable to process request - filter is mandatory for {nameof(FileSystemLoader)}.");
            }
            else
            {
                // TODO: Ihan eka pitäisi tutkia löytyykö cachesta!!!


                // should we use .bundle file first?
                bool useBundle = false;
                Bundle bundle = null;
                if (context.ContentInfo.AllowBundling)
                {
                    if (TryGetBundle(context, out bundle))
                    {
                        useBundle = true;
                    }
                }

                // ...if .bundle file is enabled -> try loading by using it.
                if (useBundle)
                {
                    content = LoadUsingFilter(bundle, context);
                }

                // finally load by filter
                if (content == null)
                {
                    content = LoadUsingFilter(context);
                }

                // TODO: Täällä jos löytyi, niin content pitäisi laittaa cacheen!!!

            }

            return ContentLoaderResponse.CreateResponse(content, context, _options1);
        }

        private bool TryGetBundle(ContentLoadingContext context, out Bundle bundle)
        {
            bundle = _cache.Get<Bundle>(_options2.Caching, context);
            if (bundle != null)
            {
                return true;
            }

            string path = PathFor(context.Theme, context.ContentInfo, Bundle.FileName);         
            IFileInfo file = _provider.GetFileInfo(path);
            if (!file.Exists)
            {
                return false;
            }

            string json = File.ReadAllText(file.PhysicalPath);
            bundle = JsonConvert.DeserializeObject<Bundle>(json);
            if (bundle != null)
            {
                _cache.Set<Bundle>(bundle, _options2.Caching, context);
                return true;
            }

            return false;
        }

        // lataa bundlesta filterin perusteella (jos ei ole filteriä, niin error!)
        private FileContent LoadUsingFilter(Bundle bundle, ContentLoadingContext context)
        {
            List<FileContent> content = new List<FileContent>();

            Bundle.Group group = bundle.Groups.FirstOrDefault(x => x.Filter == context.Filter);
            if (group != null)
            {
                foreach (var tmp in group.Files)
                {
                    string path = PathFor(context.Theme, context.ContentInfo, tmp);
                    IFileInfo file = _provider.GetFileInfo(path);

                    if (file.Exists)
                    {
                        var xxxx = LoadFile(file);
                        if (xxxx != null)
                        {
                            content.Add(xxxx);
                        }
                    }
                    else
                    {
                        // todo: lokata ettei fileä löydy!!
                    }

                }

                //  IFileInfo fi =
            }

           

            return null;
        }

        // lataa ilman bundlea käyttäällä filteriä (jos ei ole filteriä, niin error!)
        private FileContent LoadUsingFilter(ContentLoadingContext context)
        {
            return null;
        }

        private FileContent LoadFile(IFileInfo file)
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
