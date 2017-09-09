using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame.Loaders.FileSystem
{
    public class FileSystemContentLoader : IJsLoader, ICssLoader
    {
        public FileSystemContentLoader(ChameOptions options1, FileSystemContentLoaderOptions options2)
        {
            
        }

        public int Priority => 0;

        public Task<ResponseContent> LoadAsync(ChameContext context)
        {
            throw new NotImplementedException();
        }
    }
}
