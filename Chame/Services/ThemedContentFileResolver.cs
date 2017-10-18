using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chame.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Chame.Services
{
    /// <summary>
    /// Resolves which content files should be used with which themes.
    /// </summary>
    internal sealed class ThemedContentFileResolver
    {
        private readonly FileSystemLoaderOptions _options;
        private readonly ContentCache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ThemedContentFileResolver> _logger;

        public ThemedContentFileResolver(IOptions<FileSystemLoaderOptions> options, ContentCache cache, IHostingEnvironment env, ILogger<ThemedContentFileResolver> logger)
        {
            _options = options.Value;
            _cache = cache;
            _env = env;
            _logger = logger;
        }

        public ContentFile[] GetFiles(ContentLoadingContext context)
        {
            List<ContentFile> files = new List<ContentFile>();

            ContentSchema schema = null;

            if (UseCache)
            {
                schema = _cache.Get<ContentSchema>(context);
            }

            if (_options.UseContentSchemaFile)
            {
                // Load themes from settings file and cache findings for later usage.
                schema = LoadThemes();
                if (schema != null)
                {
                    if (UseCache)
                    {
                        _cache.Set<ContentSchema>(schema, _options.CacheAbsoluteExpirationRelativeToNow, context);
                    }
                }
            }
            else
            {
                // Load themes by using an external function - these findings won't be cached!
                if (_options.ContentSchemaResolver == null)
                {
                    _logger.LogError(string.Format("{0} Func is null.", nameof(_options.ContentSchemaResolver)));
                }
                else
                {
                    schema = _options.ContentSchemaResolver(context);
                }
            }

            if (schema == null)
            {
                _logger.LogError(string.Format("No content found for the requested theme '{0}'.", context.Theme));
            }
            else
            {
                // Common files for all themes.
                switch (context.Category)
                {
                    case ContentCategory.Css:
                        files.AddRange(schema.CssFiles);
                        break;
                    case ContentCategory.Js:
                        files.AddRange(schema.JsFiles);
                        break;
                }

                // Resolve theme-specific files.
                ContentFileTheme theme = schema.Themes.FirstOrDefault(x => x.Name == context.Theme);
                if (theme == null)
                {
                    _logger.LogWarning(string.Format("No content found for the requested theme '{0}'.", context.Theme));
                }
                else
                {
                    switch (context.Category)
                    {
                        case ContentCategory.Css:
                            files.AddRange(theme.CssFiles);
                            break;
                        case ContentCategory.Js:
                            files.AddRange(theme.JsFiles);
                            break;
                    }
                }
            }

            return files.ToArray();
        }

        /// <summary>
        /// Is caching enabled or not?
        /// </summary>
        private bool UseCache
        {
            get { return _options.IsCachingEnabled(_env); }
        }

        /// <summary>
        /// Loads themes from file.
        /// </summary>
        private ContentSchema LoadThemes()
        {
            if (string.IsNullOrEmpty(_options.ContentSchemaFile))
            {
                _logger.LogError("ThemesFile is missing.");
                return null;
            }

            // This is a file somewhere under wwwroot...
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options.ContentSchemaFile);

            // Check if file exists.
            if (!file.Exists)
            {
                _logger.LogError(string.Format("Requested file '{0}' does not exist.", _options.ContentSchemaFile));
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
                _logger.LogError(ex, string.Format("Failed to read file '{0}'.", file.PhysicalPath));
                throw;
            }

            // Deserialize file content.
            ContentSchema schema = JsonConvert.DeserializeObject<ContentSchema>(content);
            if (schema == null)
            {
                _logger.LogError(string.Format("Unable to deserialize JSON content from the requested file '{0}'.", _options.ContentSchemaFile));
                return null;
            }

            return schema;
        }

    }
}
