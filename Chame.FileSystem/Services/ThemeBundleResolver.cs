using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Chame.FileSystem.Services
{
    internal sealed class ThemeBundleResolver
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

            bool useCache = _options.IsCachingEnabled(_env);
            if (useCache)
            {
                bundle = _cache.Get<ThemeBundle>(Cache.Block.ThemeBundle, context);
            }

            if (bundle == null && _options.UseSetupFile)
            {
                Setup setup = LoadSetup();

                if (setup != null)
                {
                    bundle = setup.Themes.FirstOrDefault(x => x.Name == context.Theme);
                    if (bundle != null && useCache)
                    {
                        _cache.Set<ThemeBundle>(bundle, Cache.Block.ThemeBundle, context);
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

            // This is a file somewhere under wwwroot...
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options.SetupFilePath);

            // Check if file exists.
            if (!file.Exists)
            {
                _logger.LogError(string.Format("Requested setup file '{0}' does not exist.", _options.SetupFilePath));
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
                _logger.LogError(ex, string.Format("Failed to read setup file '{0}'.", file.PhysicalPath));
                throw;
            }

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
