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
    internal sealed class ThemeResolver
    {
        private readonly FileSystemLoaderOptions _options;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ThemeResolver> _logger;
        private readonly ContentCache _cache;

        public ThemeResolver(IOptions<FileSystemLoaderOptions> options, IHostingEnvironment env, ILogger<ThemeResolver> logger, ContentCache cache)
        {
            _options = options.Value;
            _env = env;
            _logger = logger;
            _cache = cache;
        }

        public Theme GetTheme(ChameContext context)
        {
            bool useCache = _options.IsCachingEnabled(_env);

            // First try to get theme content from the cache.
            if (useCache)
            {
                Theme theme = _cache.Get<Theme>(ContentCache.Block.Theme, context);
                if (theme != null)
                {
                    return theme;
                }
            }

            if (_options.UseThemeContainerFile)
            {
                // Load theme content from settings file and cache findings for later usage.
                ThemeContainer container = LoadThemeContainerFromFile();
                if (container != null)
                {
                    Theme theme = container.Themes.FirstOrDefault(x => x.Name == context.Theme);
                    if (theme != null)
                    {
                        if (useCache)
                        {
                            _cache.Set<Theme>(theme, ContentCache.Block.Theme, context);
                        }
                        return theme;
                    }
                }
            }
            else
            {
                // Load theme bundle by using an external function - these findings won't be cached!
                if (_options.ThemeContainerGetter == null)
                {
                    _logger.LogError("LoadThemeContainer Func is null.");
                }
                else
                {
                    ThemeContainer container = _options.ThemeContainerGetter();
                    if (container != null)
                    {
                        Theme theme = container.Themes.FirstOrDefault(x => x.Name == context.Theme);
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
        /// Loads theme container from file.
        /// </summary>
        private ThemeContainer LoadThemeContainerFromFile()
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
            ThemeContainer container = JsonConvert.DeserializeObject<ThemeContainer>(content);
            if (container == null)
            {
                _logger.LogError(string.Format("Unable to deserialize JSON content from the requested file '{0}'.", _options.ThemeContainerFile));
                return null;
            }

            return container;
        }

    }
}
