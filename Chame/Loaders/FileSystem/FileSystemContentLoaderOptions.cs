using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chame.Loaders.FileSystem
{
    public class FileSystemContentLoaderOptions
    {
        public static FileSystemContentLoaderOptions CreateDefault()
        {
            FileSystemContentLoaderOptions options = new FileSystemContentLoaderOptions();

            options.UseSetupFile = true;
            options.SetupFilePath = @"\chame.json";

            return options;
        }


        public virtual string SetupFilePath { get; set; }

        public virtual bool UseSetupFile { get; set; }


        //          //   var sss = ;



    }
}
