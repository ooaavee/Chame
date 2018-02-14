using Chame.Caching;
using Chame.Themes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

            ContentLoaderResponse response = Load(context);
            return Task.FromResult(response);
        }
     
        private ContentLoaderResponse Load(ContentLoadingContext context)
        {
            FileContent content = null;

            if (string.IsNullOrEmpty(context.Filter))
            { 
                _logger.LogError($"Unable to process request - filter is mandatory for {nameof(FileSystemLoader)}.");
            }
            else
            {
                // first try to use cached content
                content = _cache.Get<FileContent>(_options2.Caching, context);
                if (content != null)
                {
                    return ContentLoaderResponse.Create(content, context, _options1.SupportETag);
                }

                // check if we should use the bundle file?
                bool useBundle = false;
                Bundle bundle = null;
                if (context.ContentInfo.AllowBundling)
                {
                    if (TryGetBundle(context, out bundle))
                    {
                        useBundle = true;
                    }
                }

                // get content by using the bunble file
                if (useBundle)
                {
                    content = GetFilesInBundle(bundle, context);
                }

                // get content from single file
                if (content == null)
                {
                    content = GetSingleFile(context);
                }

                // cache content for later usage
                if (content != null)
                {
                    _cache.Set<FileContent>(content, _options2.Caching, context);
                }
            }

            return ContentLoaderResponse.Create(content, context, _options1.SupportETag);
        }

        /// <summary>
        /// Tries to find a 'bundle.json' file for the current request.
        /// </summary>
        private bool TryGetBundle(ContentLoadingContext context, out Bundle bundle)
        {
            bundle = _cache.Get<Bundle>(_options2.Caching, context);
            if (bundle != null)
            {
                return true;
            }

            string path = GetRelativePath(context.Theme, context.ContentInfo, Bundle.FileName);         
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

        /// <summary>
        /// Reads files from the bundle. 
        /// If the bundle doesn't contain any files for the current request, a null will be returned.
        /// </summary>
        private FileContent GetFilesInBundle(Bundle bundle, ContentLoadingContext context)
        {
            Bundle.Group group = bundle.Groups.FirstOrDefault(x => x.Filter == context.Filter);
            if (group == null || !group.Files.Any())
            {
                return null;
            }

            List<byte> data = new List<byte>();

            foreach (string file in group.Files)
            {
                try
                {
                    string path = GetRelativePath(context.Theme, context.ContentInfo, file);
                    IFileInfo info = _provider.GetFileInfo(path);

                    if (info.Exists)
                    {
                        byte[] tmp = File.ReadAllBytes(info.PhysicalPath);
                        data.AddRange(tmp);
                    }
                    else
                    {
                        _logger.LogError($"Bundle file not found [{path}].");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while reading bundle file [{context.ContentInfo.Extension}/{context.Filter}/{file}].");
                    throw;
                }
            }

            FileContent content = new FileContent {Data = data.ToArray()};

            if (_options1.SupportETag)
            {
                content.ETag = CreateHttpETag(content.Data);
            }

            return content;
        }

        /// <summary>
        /// Reads a single file defined by the current request. 
        /// If the file doesn't exist, a null will be returned.
        /// </summary>
        private FileContent GetSingleFile(ContentLoadingContext context)
        {
            byte[] data = null;

            try
            {
                string path = GetRelativePath(context.Theme, context.ContentInfo, context.Filter);
                IFileInfo info = _provider.GetFileInfo(path);

                if (info.Exists)
                {
                    data = File.ReadAllBytes(info.PhysicalPath);
                }
                else
                {
                    _logger.LogDebug($"File not found [{path}].");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while reading bundle file [{context.ContentInfo.Extension}/{context.Filter}].");
                throw;
            }

            if (data == null)
            {
                return null;
            }

            FileContent content = new FileContent { Data = data };

            if (_options1.SupportETag)
            {
                content.ETag = CreateHttpETag(content.Data);
            }

            return content;
        }

        private static string GetRelativePath(ITheme theme, IContentInfo contentInfo, string fileName)
        {
            string path = $"/{theme.GetName()}/{contentInfo.Extension}/{fileName}";
            return path;
        }

        /// <summary>
        /// Calculates a HTTP ETag.
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>HTTP ETag</returns>
        private static string CreateHttpETag(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                var buffer = new StringBuilder(64);
                var hash = sha256.ComputeHash(data);
                foreach (var b in hash)
                {
                    buffer.AppendFormat("{0:X2}", b);
                }
                return buffer.ToString();
            }
        }
    }
}
