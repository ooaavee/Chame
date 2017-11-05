using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chame.Caching;
using Chame.ContentLoaders.JsAndCssFiles.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        private readonly bool _useCache;
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
            _useCache = options2.Value.Caching.IsEnabled(env);
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Content loader priority. 
        /// An execution order of content loaders are sorted by this property. 
        /// This is only meaningful if there are more than one content loaders!
        /// </summary>
        public double Priority => 0;

        /// <summary>
        /// Content-type extensions that are supported by the content loader.
        /// </summary>
        /// <returns>supported content-type extensions</returns>
        public IEnumerable<string> Supports()
        {
            yield return "js";
            yield return "css";
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
            FileContent content = null;

            // first try to use cached content
            if (_useCache)
            {
                content = _cache.Get<FileContent>(context);
                if (content != null)
                {
                    return ContentLoaderResponse.CreateResponse(content, context, _options1);
                }
            }

            // read content files and optionally cache the content
            ContentFile[] files = GetFiles(context);
            if (files.Any())
            {
                content = GetContent(files, context);
                if (_useCache)
                {
                    _cache.Set<FileContent>(content, _options2.Caching.AbsoluteExpirationRelativeToNow, context);
                }
            }

            return ContentLoaderResponse.CreateResponse(content, context, _options1);
        }

        private FileContent GetContent(IEnumerable<ContentFile> files, ContentLoadingContext context)
        {
            var data = new List<byte>();
            foreach (ContentFile file in Filter(files, context.Filter))
            {
                var tmp = ReadAllBytes(file);
                if (tmp != null)
                {
                    data.AddRange(tmp);
                }
            }

            var content = new FileContent { Data = data.ToArray() };

            if (_options1.SupportETag)
            {
                content.ETag = HttpETagHelper.Calculate(content.Data);
            }

            return content;
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

        private byte[] ReadAllBytes(ContentFile file)
        {
            IFileInfo fi = _env.WebRootFileProvider.GetFileInfo(file.Path);
            if (fi.Exists)
            {
                try
                {
                    return File.ReadAllBytes(fi.PhysicalPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Format("Failed to read a content file '{0}'.", fi.PhysicalPath));
                    throw;
                }
            }
            _logger.LogError(string.Format("Content file '{0}' does not exist.", file.Path));
            return null;
        }

        private ContentFile[] GetFiles(ContentLoadingContext context)
        {
            List<ContentFile> files = new List<ContentFile>();

            ContentSchema schema = null;

            if (_useCache)
            {
                schema = _cache.Get<ContentSchema>(context);
            }

            if (_options2.UseContentSchemaFile)
            {
                // Load themes from settings file and cache findings for later usage.
                schema = LoadSchema();
                if (schema != null)
                {
                    if (_useCache)
                    {
                        _cache.Set<ContentSchema>(schema, _options2.Caching.AbsoluteExpirationRelativeToNow, context);
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
                string themeName = context.Theme.GetName();
                ContentFileTheme theme = schema.Themes.FirstOrDefault(x => x.Id == themeName);
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
                    var json = File.ReadAllText(file.PhysicalPath);
                    return Deserialize(json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Format("Failed to read file '{0}'.", file.PhysicalPath));
                    throw;
                }
            }

            _logger.LogError(string.Format("Requested file '{0}' does not exist.", _options2.ContentSchemaFile));
            return null;
        }

        private static ContentSchema Deserialize(string json)
        {
            var schema = JsonConvert.DeserializeObject<ContentSchema>(json);
            if (schema == null)
            {
                throw new InvalidOperationException("Unable to deserialize JSON content.");
            }
            return schema;
        }

    }
}
