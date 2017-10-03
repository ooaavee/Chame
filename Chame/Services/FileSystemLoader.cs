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
        private readonly ChameOptions _chameOptions;
        private readonly FileSystemLoaderOptions _fileSystemLoaderOptions;
        private readonly ContentCache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<FileSystemLoader> _logger;

        public FileSystemLoader(ThemeResolver resolver, IOptions<ChameOptions> chameOptions, IOptions<FileSystemLoaderOptions> fileSystemLoaderOptions, ContentCache cache, IHostingEnvironment env, ILogger<FileSystemLoader> logger)
        {
            _resolver = resolver;
            _chameOptions = chameOptions.Value;
            _fileSystemLoaderOptions = fileSystemLoaderOptions.Value;
            _cache = cache;
            _env = env;          
            _logger = logger;
        }

        public int Priority => 1073741823;

        public Task<ResponseContent> LoadAsync(ChameContext context)
        {
            // First try to use cached content.
            if (_fileSystemLoaderOptions.IsCachingEnabled(_env))
            {
                ContentContainer cached = _cache.Get<ContentContainer>(ContentCache.Block.ContentContainer, context);
                if (cached != null)
                {
                    return GetResponseContent(cached, context).AsTask();
                }
            }

            // Get theme.
            // Return HTTP NotFound if theme was not found.
            Theme theme = _resolver.GetTheme(context);
            if (theme == null)
            {
                return ResponseContent.NotFound().AsTask();
            }

            // Get files to use.
            List<ThemeFile> files;
            switch (context.Category)
            {
                case ContentCategory.Css:
                    files = theme.Css;
                    break;
                case ContentCategory.Js:
                    files = theme.Js;
                    break;
                default:
                    throw new InvalidOperationException("fuck");
            }

            //
            // JOS THEMESSA EI OLE FILEJÄ --->> PALAUTETAAN BLANKKO, EI NOT FOUNDIA!
            //

            // Return HTTP NotFound if there are no files in the theme.         
            if (files == null || !files.Any())
            {
                return ResponseContent.NotFound().AsTask();
            }

            // Read bundle content and optionally cache it.
            ContentContainer content = ReadBundleContent(files, context);
            bool useCache = _fileSystemLoaderOptions.IsCachingEnabled(_env);
            if (useCache)
            {
                _cache.Set<ContentContainer>(content, ContentCache.Block.ContentContainer, context);
            }

            return GetResponseContent(content, context).AsTask();
        }

        private ResponseContent GetResponseContent(ContentContainer container, ChameContext context)
        {            
            if (_chameOptions.SupportETag)
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
            if (_chameOptions.SupportETag)
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
