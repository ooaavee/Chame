using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Chame.Loaders.FileSystem
{
    internal class ThemeBundleResolver
    {
        private readonly FileSystemContentLoaderOptions _options;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ThemeBundleResolver> _logger;
        private readonly Cache _cache;

        public ThemeBundleResolver(IOptions<FileSystemContentLoaderOptions> options, IHostingEnvironment env, ILogger<ThemeBundleResolver> logger, Cache cache)
        {
            _options = options.Value;
            _env = env;
            _logger = logger;
            _cache = cache;
        }

        public ThemeBundle GetThemedBundle(ChameContext context)
        {
            ThemeBundle bundle = null;

            if (_options.UseCache)
            {
                bundle = _cache.Get<ThemeBundle>(Cache.CacheBlock.ThemeBundle, context);
            }

            if (bundle == null && _options.UseSetupFile)
            {
                Setup setup = LoadSetup();
                if (setup != null)
                {
                    bundle = setup.Themes.FirstOrDefault(x => x.Name == context.Theme);
                    if (bundle != null && _options.UseCache)
                    {
                        _cache.Set<ThemeBundle>(bundle, Cache.CacheBlock.ThemeBundle, context);
                    }
                }
            }

            if (bundle == null)
            {
                _logger.LogWarning(string.Format("Unable to find setup for the file system content loader. The requested theme was '{0}'.", context.Theme));
            }

            return bundle;
        }

        /// <summary>
        /// Loads setup file for the file system content loader.
        /// </summary>
        private Setup LoadSetup()
        {
            if (string.IsNullOrEmpty(_options.SetupFilePath))
            {
                _logger.LogError("SetupFilePath is missing.");
                return null;
            }

            // Check if file exists.
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options.SetupFilePath);
            if (!file.Exists)
            {
                _logger.LogError(string.Format("Requested setup file '{0}' does not exist.", _options.SetupFilePath));
                return null;
            }

            // Read file content.
            string content = System.IO.File.ReadAllText(file.PhysicalPath);

            // Deserialize file content.
            Setup setup = JsonConvert.DeserializeObject<Setup>(content);
            if (setup == null)
            {
                _logger.LogError(string.Format("Unable to deserialize JSON content from the requested setup file '{0}' does not exist.", _options.SetupFilePath));
                return null;
            }

            return setup;
        }

    }
}
