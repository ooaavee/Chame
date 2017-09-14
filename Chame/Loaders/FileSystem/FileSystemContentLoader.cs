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
            Setup setup;
            if (!TryGetSetup(out setup))
            {
                return null;
            }

            string theme = "";

            string[] paths;

            switch (context.Category)
            {
                case ContentCategory.Css:
                    paths = setup.
                    break;
                case ContentCategory.Js:
                    break;
                default:
                    break;
                    ;
            }


            //var sss1 = _env.WebRootFileProvider.GetDirectoryContents(_options2.JsDirectory).ToArray();

            //var sss2 = _env.WebRootFileProvider.GetDirectoryContents(_options2.CssDirectory).ToArray();


            return null;
        }

        private bool TryGetSetup(out Setup setup)
        {
            setup = null;

            if (_options2.UseSetupFile)
            {
                if (!TryGetSetupFromFile(out setup))
                {
                    _logger.LogError("Unable to read setup from file system.");
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to load setup file.
        /// </summary>
        private bool TryGetSetupFromFile(out Setup setup)
        {
            setup = null;

            // Check if file exists.
            IFileInfo file = _env.WebRootFileProvider.GetFileInfo(_options2.SetupFilePath);
            if (!file.Exists)
            {
                return false;
            }

            // Read file content.
            string content = System.IO.File.ReadAllText(file.PhysicalPath);

            // Deserialize file content.
            setup = JsonConvert.DeserializeObject<Setup>(content);
            if (setup == null)
            {
                return false;
            }

            return true;
        }

    }
}
