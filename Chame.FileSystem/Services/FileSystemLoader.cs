using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.FileSystem.Services
{
    internal sealed class FileSystemLoader : IJsLoader, ICssLoader
    {
        private readonly ThemeBundleResolver _resolver;
        private readonly ChameOptions _chameOptions;
        private readonly FileSystemContentLoaderOptions _loaderOptions;
        private readonly Cache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<FileSystemLoader> _logger;

        public FileSystemLoader(ThemeBundleResolver resolver, IOptions<ChameOptions> chameOptions, IOptions<FileSystemContentLoaderOptions> loaderOptions, Cache cache, IHostingEnvironment env, ILogger<FileSystemLoader> logger)
        {
            _resolver = resolver;
            _chameOptions = chameOptions.Value;
            _loaderOptions = loaderOptions.Value;
            _cache = cache;
            _env = env;          
            _logger = logger;
        }

        public int Priority => 1073741823;

        public Task<ResponseContent> LoadAsync(ChameContext context)
        {
            // First try to use cached content.
            if (_loaderOptions.IsCachingEnabled(_env))
            {
                BundleContent cached = _cache.Get<BundleContent>(Cache.Block.BundleContent, context);
                if (cached != null)
                {
                    return GetResponseContent(cached, context).AsTask();
                }
            }

            // Get themed bundle.
            // Return HTTP NotFound if bundle was not found.
            ThemeBundle bundle = _resolver.GetThemedBundle(context);
            if (bundle == null)
            {
                return ResponseContent.NotFound().AsTask();
            }

            // Get files to use.
            List<ThemeBundle.BundleFile> bundleFiles;
            switch (context.Category)
            {
                case ContentCategory.Css:
                    bundleFiles = bundle.Css;
                    break;
                case ContentCategory.Js:
                    bundleFiles = bundle.Js;
                    break;
                default:
                    throw new InvalidOperationException("fuck");
            }

            // Return HTTP NotFound if there are no files in the bundle.
            if (bundleFiles == null || !bundleFiles.Any())
            {
                return ResponseContent.NotFound().AsTask();
            }


            // Read bundle content and optionally cache it.
            BundleContent content = ReadBundleContent(bundleFiles, context);
            bool useCache = _loaderOptions.IsCachingEnabled(_env);
            if (useCache)
            {
                _cache.Set<BundleContent>(content, Cache.Block.BundleContent, context);
            }

            return GetResponseContent(content, context).AsTask();
        }

        private ResponseContent GetResponseContent(BundleContent content, ChameContext context)
        {            
            if (_chameOptions.UseETag)
            {
                if (context.ETag != null && content.ETag != null)
                {
                    if (context.ETag == content.ETag)
                    {
                        return ResponseContent.NotModified();
                    }
                }
            }

            return content.ETag == null ? 
                ResponseContent.Ok(content.Content, content.Encoding) : 
                ResponseContent.Ok(content.Content, content.Encoding, content.ETag);
        }

        private BundleContent ReadBundleContent(IEnumerable<ThemeBundle.BundleFile> bundleFiles, ChameContext context)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (ThemeBundle.BundleFile file in GetFiles(bundleFiles, context))
            {
                string content = ReadFile(file);
                if (content != null)
                {
                    buffer.AppendLine(content);
                }
            }

            string data = buffer.ToString();

            string eTag = null;
            if (_chameOptions.UseETag)
            {
                eTag = CalculateETag(data);
            }

            return new BundleContent {Encoding = Encoding.UTF8, Content = data, ETag = eTag};
        }

        private static IEnumerable<ThemeBundle.BundleFile> GetFiles(IEnumerable<ThemeBundle.BundleFile> bundleFiles, ChameContext context)
        {
            Regex regex = null;

            foreach (ThemeBundle.BundleFile file in bundleFiles)
            {
                if (file.Filter == null)
                {
                    yield return file;
                }
                else if (context.Filter == null && file.Filter == null)
                {
                    yield return file;
                }
                else if (context.Filter != null && file.Filter != null)
                {
                    if (regex == null)
                    {
                        regex = new Regex(context.Filter);
                    }

                    if (regex.IsMatch(file.Filter))
                    {
                        yield return file;
                    }
                }
            }
        }

        private string ReadFile(ThemeBundle.BundleFile bundleFile)
        {
            // This is a file somewhere under wwwroot...
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(bundleFile.Path);

            // Check if the file exists.
            if (!file.Exists)
            {
                _logger.LogError(string.Format("Bundle file '{0}' does not exist.", bundleFile.Path));
                return null;
            }

            // Read the file.
            string content;
            try
            {
                content = File.ReadAllText(file.PhysicalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Failed to read bundle file '{0}'.", file.PhysicalPath));
                throw;
            }

            return content;
        }

        private static string CalculateETag(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(data));
                var buffer = new StringBuilder();
                foreach (var b in hash)
                {
                    buffer.AppendFormat("{0:X2}", b);
                }
                return buffer.ToString();
            }
        }

    }
}
