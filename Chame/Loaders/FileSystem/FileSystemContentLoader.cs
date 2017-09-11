using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Chame.Loaders.FileSystem
{
    public class FileSystemContentLoader : IJsLoader, ICssLoader
    {
        private readonly ChameOptions _options1;
        private readonly FileSystemContentLoaderOptions _options2;
        private readonly IHostingEnvironment _env;

        public FileSystemContentLoader(ChameOptions options1, FileSystemContentLoaderOptions options2, IHostingEnvironment env)
        {
            _options1 = options1;
            _options2 = options2;
            _env = env;
        }

        public int Priority => 0;

        public Task<ResponseContent> LoadAsync(ChameContext context)
        {

            var sss1 = _env.WebRootFileProvider.GetDirectoryContents(_options2.JsDirectory).ToArray();

            var sss2 = _env.WebRootFileProvider.GetDirectoryContents(_options2.CssDirectory).ToArray();


            return null;
        }
    }
}
