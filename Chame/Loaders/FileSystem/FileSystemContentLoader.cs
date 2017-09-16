using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Chame.Loaders.FileSystem
{
    public class FileSystemContentLoader : IJsLoader, ICssLoader
    {
        private readonly ChameOptions _options1;
        private readonly FileSystemContentLoaderOptions _options2;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<FileSystemContentLoader> _logger;

        public FileSystemContentLoader(ChameOptions options1, FileSystemContentLoaderOptions options2, IHostingEnvironment env, ILogger<FileSystemContentLoader> logger)
        {
            _options1 = options1;
            _options2 = options2;
            _env = env;
            _logger = logger;
        }

        public int Priority => 1073741823;

        public Task<ResponseContent> LoadAsync(ChameContext context)
        {
            
            Setup.ThemedBundle bundle = GetThemedBundle(context);
            if (bundle == null)
            {
                return Task.FromResult(ResponseContent.NotFound());
            }

            List<Setup.BundleFile> paths;
            switch (context.Category)
            {
                case ContentCategory.Css:
                    paths = bundle.Css;
                    break;
                case ContentCategory.Js:
                    paths = bundle.Js;
                    break;
                default:
                    throw new InvalidOperationException("fuck");
            }

            if (paths == null || !paths.Any())
            {
                return null;
            }

            


            //Setup setup;
            //if (!TryGetSetup(out setup))
            //{
            //    _logger.LogError("Unable to find setup for the file system content loader.");
            //    return null;
            //}

            //Setup.ThemedBundle bundle = setup.Themes.FirstOrDefault(theme => theme.Name == context.Theme);
            //if (bundle == null)
            //{
            //    return null;
            //}

            //var sss1 = _env.WebRootFileProvider.GetDirectoryContents(_options2.JsDirectory).ToArray();

            //var sss2 = _env.WebRootFileProvider.GetDirectoryContents(_options2.CssDirectory).ToArray();


            return null;
        }

        private Setup.ThemedBundle GetThemedBundle(ChameContext context)
        {
            string theme = context.Theme;

            if (_options2.UseSetupFile)
            {
                Setup setup = LoadSetupFromFile();
                Setup.ThemedBundle bundle = setup?.Themes.FirstOrDefault(x => x.Name == theme);
                if (bundle != null)
                {
                    return bundle;
                }
            }

            _logger.LogWarning(string.Format("Unable to find setup for the file system content loader. The requested theme was '{0}'.", theme));
            return null;
        }
    
        /// <summary>
        /// Loads setup file for the file system content loader.
        /// </summary>
        private Setup LoadSetupFromFile()
        {
            if (string.IsNullOrEmpty(_options2.SetupFilePath))
            {
                _logger.LogError("SetupFilePath is missing.");
                return null;
            }

            // Check if file exists.
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options2.SetupFilePath);
            if (!file.Exists)
            {
                _logger.LogError(string.Format("Requested setup file '{0}' does not exist.", _options2.SetupFilePath));
                return null;
            }

            // Read file content.
            string content = System.IO.File.ReadAllText(file.PhysicalPath);

            // Deserialize file content.
            Setup setup = JsonConvert.DeserializeObject<Setup>(content);
            if (setup == null)
            {
                _logger.LogError(string.Format("Unable to deserialize JSON content from the requested setup file '{0}' does not exist.", _options2.SetupFilePath));
                return null;
            }

            return setup;
        }

    }
}
