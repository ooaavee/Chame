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
    internal sealed class FileSystemLoader : IJsLoader, ICssLoader
    {
        private readonly ThemeResolver _resolver;
        private readonly ChameOptions _options1;
        private readonly ChameFileSystemLoaderOptions _options2;
        private readonly ChameMemoryCache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<FileSystemLoader> _logger;

        public FileSystemLoader(ThemeResolver resolver, IOptions<ChameOptions> options1, IOptions<ChameFileSystemLoaderOptions> options2, ChameMemoryCache cache, IHostingEnvironment env, ILogger<FileSystemLoader> logger)
        {
            _resolver = resolver;
            _options1 = options1.Value;
            _options2 = options2.Value;
            _cache = cache;
            _env = env;          
            _logger = logger;
        }

        public int Priority => 1073741823;

        public Task<ResponseContent> LoadAsync(ChameContext context)
        {
            ResponseContent response = Load(context);
            return Task.FromResult(response);
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

            // Get theme.
            // Return HTTP NotFound if theme was not found.
            Theme theme = _resolver.GetTheme(context);
            if (theme == null)
            {
                return ResponseContent.NotFound();
            }

            // Get files to use.
            List<ThemeFile> files;
            switch (context.Category)
            {
                case ContentCategory.Css:
                    files = theme.CssFiles;
                    break;
                case ContentCategory.Js:
                    files = theme.JsFiles;
                    break;
                default:
                    throw new InvalidOperationException("fuck");
            }

            ContentContainer content;

            // Read bundle content and optionally cache it.
            if (files != null && files.Any())
            {
                content = ReadBundleContent(files, context);
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

            return container.ETag == null ? 
                ResponseContent.Ok(container.Content, container.Encoding) : 
                ResponseContent.Ok(container.Content, container.Encoding, container.ETag);
        }

        private ContentContainer ReadBundleContent(IEnumerable<ThemeFile> files, ChameContext context)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (ThemeFile file in FilterFiles(files, context))
            {
                string s = ReadFile(file);
                if (s != null)
                {
                    buffer.AppendLine(s);
                }
            }

            string content = buffer.ToString();

            string eTag = null;
            if (_options1.SupportETag)
            {
                eTag = GetETag(content);
            }

            return new ContentContainer(content, eTag);
        }

        private static IEnumerable<ThemeFile> FilterFiles(IEnumerable<ThemeFile> files, ChameContext context)
        {
            Regex regex = null;

            foreach (ThemeFile file in files)
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

        private string ReadFile(ThemeFile file)
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
