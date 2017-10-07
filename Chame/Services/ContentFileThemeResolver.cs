using System;
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
    internal sealed class ContentFileThemeResolver
    {
        private readonly ChameFileSystemLoaderOptions _options;
        private readonly ChameMemoryCache _cache;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ContentFileThemeResolver> _logger;

        public ContentFileThemeResolver(IOptions<ChameFileSystemLoaderOptions> options, ChameMemoryCache cache, IHostingEnvironment env, ILogger<ContentFileThemeResolver> logger)
        {
            _options = options.Value;
            _cache = cache;
            _env = env;
            _logger = logger;
        }

        public ContentFileTheme GetTheme(ChameContext context)
        {
            // First try to get theme content from the cache.
            if (UseCache)
            {
                ContentFileTheme theme = _cache.Get<ContentFileTheme>(context);
                if (theme != null)
                {
                    return theme;
                }
            }

            if (_options.UseThemeContainerFile)
            {
                // Load theme content from settings file and cache findings for later usage.
                ContentFileThemeContainer container = LoadThemeContainerFromFile();
                if (container != null)
                {
                    ContentFileTheme theme = container.Themes.FirstOrDefault(x => x.Name == context.Theme);
                    if (theme != null)
                    {
                        if (UseCache)
                        {
                            _cache.Set<ContentFileTheme>(theme, _options.CacheAbsoluteExpirationRelativeToNow, context);
                        }
                        return theme;
                    }
                }
            }
            else
            {
                // Load theme bundle by using an external function - these findings won't be cached!
                if (_options.ThemeContainerLoader == null)
                {
                    _logger.LogError(string.Format("{0} Func is null.", nameof(_options.ThemeContainerLoader)));
                }
                else
                {
                    ContentFileThemeContainer container = _options.ThemeContainerLoader(context);
                    if (container != null)
                    {
                        ContentFileTheme theme = container.Themes.FirstOrDefault(x => x.Name == context.Theme);
                        if (theme != null)
                        {
                            return theme;
                        }
                    }
                }
            }

            _logger.LogWarning(string.Format("No content found for the requested theme '{0}'.", context.Theme));

            return null;
        }

        /// <summary>
        /// Is caching enabled or not?
        /// </summary>
        private bool UseCache
        {
            get { return _options.IsCachingEnabled(_env); }
        }

        /// <summary>
        /// Loads theme container from file.
        /// </summary>
        private ContentFileThemeContainer LoadThemeContainerFromFile()
        {
            if (string.IsNullOrEmpty(_options.ThemeContainerFile))
            {
                _logger.LogError("ThemeContainerFile is missing.");
                return null;
            }

            // This is a file somewhere under wwwroot...
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options.ThemeContainerFile);

            // Check if file exists.
            if (!file.Exists)
            {
                _logger.LogError(string.Format("Requested file '{0}' does not exist.", _options.ThemeContainerFile));
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
            ContentFileThemeContainer container = JsonConvert.DeserializeObject<ContentFileThemeContainer>(content);
            if (container == null)
            {
                _logger.LogError(string.Format("Unable to deserialize JSON content from the requested file '{0}'.", _options.ThemeContainerFile));
                return null;
            }

            return container;
        }

    }
}
