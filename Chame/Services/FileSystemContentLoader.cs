using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chame.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.Services
{
    /// <summary>
    /// Loads JavaScript and CSS content from the filesystem.
    /// </summary>
    internal sealed class FileSystemContentLoader : IJsContentLoader, ICssContentLoader
    {
        private readonly ThemedContentFileResolver _resolver;
        private readonly ChameOptions _options1;
        private readonly ChameFileSystemLoaderOptions _options2;
        private readonly ChameMemoryCache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<FileSystemContentLoader> _logger;

        public FileSystemContentLoader(ThemedContentFileResolver resolver, IOptions<ChameOptions> options1, IOptions<ChameFileSystemLoaderOptions> options2, ChameMemoryCache cache, IHostingEnvironment env, ILogger<FileSystemContentLoader> logger)
        {
            _resolver = resolver;
            _options1 = options1.Value;
            _options2 = options2.Value;
            _cache = cache;
            _env = env;          
            _logger = logger;
        }

        public int Priority => 0;

        public Task<ResponseContent> LoadAsync(ChameContext context)
        {
            return Task.FromResult(Load(context));
        }

        /// <summary>
        /// Is caching enabled or not?
        /// </summary>
        private bool UseCache
        {
            get { return _options2.IsCachingEnabled(_env); }
        }

        private ResponseContent Load(ChameContext context)
        {
            // First try to use cached content.
            if (UseCache)
            {
                ContentContainer cached = _cache.Get<ContentContainer>(context);
                if (cached != null)
                {
                    return GetResponseContent(cached, context);
                }
            }

            // Get files to use.
            ContentFile[] files = _resolver.GetFiles(context);

            // Read content files and optionally cache the content.
            ContentContainer content;
            if (files.Any())
            {
                content = GetContent(files, context);
                if (UseCache)
                {
                    _cache.Set<ContentContainer>(content, _options2.CacheAbsoluteExpirationRelativeToNow, context);
                }
            }
            else
            {
                content = ContentContainer.Empty();
            }

            return GetResponseContent(content, context);
        }

        private ResponseContent GetResponseContent(ContentContainer container, ChameContext context)
        {            
            if (_options1.SupportETag)
            {
                if (context.ETag != null && container.ETag != null)
                {
                    if (context.ETag == container.ETag)
                    {
                        return ResponseContent.NotModified();
                    }
                }
            }
            return container.ETag == null ? ResponseContent.Ok(container.Content, container.Encoding) : ResponseContent.Ok(container.Content, container.Encoding, container.ETag);
        }

        private ContentContainer GetContent(IEnumerable<ContentFile> files, ChameContext context)
        {
            StringBuilder buffer = new StringBuilder();

            foreach (ContentFile file in Filter(files, context.Filter))
            {
                string s = ReadFile(file);
                if (s != null)
                {
                    buffer.AppendLine(s);
                }
            }

            string content = buffer.ToString();

            string eTag = _options1.SupportETag ? GetETag(content) : null;

            return new ContentContainer(content, eTag);
        }

        private static IEnumerable<ContentFile> Filter(IEnumerable<ContentFile> files, string filter)
        {
            foreach (ContentFile file in files)
            {
                if (filter == null)
                {
                    if (string.IsNullOrEmpty(file.Filter))
                    {
                        yield return file;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(file.Filter))
                    {
                        if (Regex.IsMatch(file.Filter, filter))
                        {
                            yield return file;
                        }
                    }
                }
            }
        }

        private string ReadFile(ContentFile file)
        {
            // This is a file somewhere under wwwroot...
            IFileInfo fi = _env.WebRootFileProvider.GetFileInfo(file.Path);

            // Check if the file exists.
            if (!fi.Exists)
            {
                _logger.LogError(string.Format("Bundle file '{0}' does not exist.", file.Path));
                return null;
            }

            // Read the file.
            string content;
            try
            {
                content = File.ReadAllText(fi.PhysicalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Failed to read bundle file '{0}'.", fi.PhysicalPath));
                throw;
            }

            return content;
        }

        private static string GetETag(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(content));
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
