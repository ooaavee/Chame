using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chame.Caching;
using Chame.ContentLoaders.JsAndCssFiles.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chame.ContentLoaders.JsAndCssFiles
{
    /// <summary>
    /// Loads JavaScript and CSS files from the filesystem.
    /// </summary>
    public class JsAndCssFileLoader : IContentLoader
    {
        private readonly ContentLoaderOptions _options1;
        private readonly JsAndCssFileLoaderOptions _options2;
        private readonly ContentCache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<JsAndCssFileLoader> _logger;

        public JsAndCssFileLoader(IOptions<ContentLoaderOptions> options1, IOptions<JsAndCssFileLoaderOptions> options2, ContentCache cache, IHostingEnvironment env, ILogger<JsAndCssFileLoader> logger)
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
            _env = env;          
            _logger = logger;
        }

        public double Priority => 0;

        public IEnumerable<string> ContentTypeExtensions()
        {
            yield return "js";
            yield return "css";
        }

        public Task<TextContent> LoadContentAsync(ContentLoadingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(Load(context));
        }

        /// <summary>
        /// Is caching enabled?
        /// </summary>
        private bool IsCacheEnabled
        {
            get
            {
                switch (_options2.CachingMode)
                {
                    case CachingModes.Disabled:
                        return false;
                    case CachingModes.Enabled:
                        return true;
                    case CachingModes.EnabledButDisabledOnDev:
                        return !_env.IsDevelopment();
                    default:
                        return false;
                }
            }
        }

        private TextContent Load(ContentLoadingContext context)
        {
            FileContent content;

            // First try to use cached content.
            if (IsCacheEnabled)
            {
                content = _cache.Get<FileContent>(context);
                if (content != null)
                {
                    return GetResponseContent(content, context);
                }
            }

            // Get files to use.
            ContentFile[] files = GetFiles(context);

            // Read content files and optionally cache the content.
            if (files.Any())
            {
                content = GetContent(files, context);
                if (IsCacheEnabled)
                {
                    _cache.Set<FileContent>(content, _options2.CacheAbsoluteExpirationRelativeToNow, context);
                }
            }
            else
            {
                content = new FileContent {Value = string.Empty};
            }

            return GetResponseContent(content, context);
        }

        private TextContent GetResponseContent(FileContent container, ContentLoadingContext context)
        {
            if (_options1.SupportETag && context.ETag != null && container.ETag != null && context.ETag == container.ETag)
            {
                return TextContent.NotModified();
            }
            return container.ETag == null ? 
                TextContent.Ok(container.Value, Encoding.UTF8) : 
                TextContent.Ok(container.Value, Encoding.UTF8, container.ETag);
        }

        private FileContent GetContent(IEnumerable<ContentFile> files, ContentLoadingContext context)
        {
            var buf = new StringBuilder();
            foreach (ContentFile file in Filter(files, context.Filter))
            {
                buf.AppendLine(ReadAllText(file));
            }
            return new FileContent
            {
                Value = buf.ToString(),
                ETag = _options1.SupportETag ? GetETag(buf.ToString()) : null
            };
        }

        private static IEnumerable<ContentFile> Filter(IEnumerable<ContentFile> files, string filter)
        {
            foreach (var file in files)
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

        private string ReadAllText(ContentFile file)
        {
            IFileInfo fi = _env.WebRootFileProvider.GetFileInfo(file.Path);
            if (fi.Exists)
            {
                try
                {
                    return File.ReadAllText(fi.PhysicalPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Format("Failed to read a content file '{0}'.", fi.PhysicalPath));
                    throw;
                }
            }
            else
            {
                _logger.LogError(string.Format("Content file '{0}' does not exist.", file.Path));
            }
            return string.Empty;
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

        private ContentFile[] GetFiles(ContentLoadingContext context)
        {
            List<ContentFile> files = new List<ContentFile>();

            ContentSchema schema = null;

            if (IsCacheEnabled)
            {
                schema = _cache.Get<ContentSchema>(context);
            }

            if (_options2.UseContentSchemaFile)
            {
                // Load themes from settings file and cache findings for later usage.
                schema = LoadSchema();
                if (schema != null)
                {
                    if (IsCacheEnabled)
                    {
                        _cache.Set<ContentSchema>(schema, _options2.CacheAbsoluteExpirationRelativeToNow, context);
                    }
                }
            }
            else
            {
                // Load themes by using an external function - these findings won't be cached!
                if (_options2.ContentSchemaResolver == null)
                {
                    _logger.LogError(string.Format("{0} Func is null.", nameof(_options2.ContentSchemaResolver)));
                }
                else
                {
                    schema = _options2.ContentSchemaResolver(context);
                }
            }

            if (schema == null)
            {
                _logger.LogError(string.Format("No content found for the requested theme '{0}'.", context.Theme));
            }
            else
            {
                // Common files for all themes.
                switch (context.ContentInfo.Extension)
                {
                    case "css":
                        files.AddRange(schema.CssFiles);
                        break;
                    case "js":
                        files.AddRange(schema.JsFiles);
                        break;
                }

                // Resolve theme-specific files.
                ContentFileTheme theme = schema.Themes.FirstOrDefault(x => x.Id == context.Theme.Id);
                if (theme == null)
                {
                    _logger.LogWarning(string.Format("No content found for the requested theme '{0}'.", context.Theme));
                }
                else
                {
                    switch (context.ContentInfo.Extension)
                    {
                        case "css":
                            files.AddRange(theme.CssFiles);
                            break;
                        case "js":
                            files.AddRange(theme.JsFiles);
                            break;
                    }
                }
            }

            return files.ToArray();
        }
     
        /// <summary>
        /// Loads themes from file.
        /// </summary>
        private ContentSchema LoadSchema()
        {
            if (string.IsNullOrEmpty(_options2.ContentSchemaFile))
            {
                _logger.LogError("ContentSchemaFile is missing.");
                return null;
            }

            // Read the file that is usually shomewhere under wwwroot...
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options2.ContentSchemaFile);
            if (file.Exists)
            {
                try
                {
                    string content = File.ReadAllText(file.PhysicalPath);
                    return ContentSchema.Deserialize(content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Format("Failed to read file '{0}'.", file.PhysicalPath));
                    throw;
                }
            }
            else
            {
                _logger.LogError(string.Format("Requested file '{0}' does not exist.", _options2.ContentSchemaFile));
                return null;
            }        
        }




        private class FileContent
        {
            /// <summary>
            /// Text content
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// HTTP ETag
            /// </summary>
            public string ETag { get; set; }
        }


    }
}
