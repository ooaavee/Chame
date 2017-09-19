using System;

namespace Chame.Loaders.FileSystem
{
    public class FileSystemContentLoaderOptions
    {

        public FileSystemContentLoaderOptions()
        {
            UseSetupFile = true;
            SetupFilePath = @"\chame.json";
            UseCache = true;
            CacheAbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 1, 0);
        }

        public string SetupFilePath { get; set; }

        public bool UseSetupFile { get; set; }

        public bool UseCache { get; set; }

        public TimeSpan CacheAbsoluteExpirationRelativeToNow { get; set; }

    }
}
