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
        private readonly ChameFileSystemLoaderOptions _options;
        private readonly ChameMemoryCache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ThemedContentFileResolver> _logger;

        public ThemedContentFileResolver(IOptions<ChameFileSystemLoaderOptions> options, ChameMemoryCache cache, IHostingEnvironment env, ILogger<ThemedContentFileResolver> logger)
        {
            _options = options.Value;
            _cache = cache;
            _env = env;
            _logger = logger;
        }

        public ContentFile[] GetFiles(ChameContext context)
        {
            List<ContentFile> files = new List<ContentFile>();

            ContentFileThemes container = null;

            if (UseCache)
            {
                container = _cache.Get<ContentFileThemes>(context);
            }

            if (_options.UseThemesFile)
            {
                // Load themes from settings file and cache findings for later usage.
                container = LoadThemes();
                if (container != null)
                {
                    if (UseCache)
                    {
                        _cache.Set<ContentFileThemes>(container, _options.CacheAbsoluteExpirationRelativeToNow, context);
                    }
                }
            }
            else
            {
                // Load themes by using an external function - these findings won't be cached!
                if (_options.ThemesLoader == null)
                {
                    _logger.LogError(string.Format("{0} Func is null.", nameof(_options.ThemesLoader)));
                }
                else
                {
                    container = _options.ThemesLoader(context);
                }
            }

            if (container == null)
            {
                _logger.LogError(string.Format("No content found for the requested theme '{0}'.", context.Theme));
            }
            else
            {
                // Common files for all themes.
                switch (context.Category)
                {
                    case ContentCategory.Css:
                        files.AddRange(container.CssFiles);
                        break;
                    case ContentCategory.Js:
                        files.AddRange(container.JsFiles);
                        break;
                }

                // Resolve theme-specific files.
                ContentFileTheme theme = container.Themes.FirstOrDefault(x => x.Name == context.Theme);
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
        private ContentFileThemes LoadThemes()
        {
            if (string.IsNullOrEmpty(_options.ThemesFile))
            {
                _logger.LogError("ThemesFile is missing.");
                return null;
            }

            // This is a file somewhere under wwwroot...
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options.ThemesFile);

            // Check if file exists.
            if (!file.Exists)
            {
                _logger.LogError(string.Format("Requested file '{0}' does not exist.", _options.ThemesFile));
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
            ContentFileThemes container = JsonConvert.DeserializeObject<ContentFileThemes>(content);
            if (container == null)
            {
                _logger.LogError(string.Format("Unable to deserialize JSON content from the requested file '{0}'.", _options.ThemesFile));
                return null;
            }

            return container;
        }

    }
}
