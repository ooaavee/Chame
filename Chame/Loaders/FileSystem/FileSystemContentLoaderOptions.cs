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

            options.JsDirectory = @"\js";

            options.CssDirectory = @"\css";

            return options;
        }

        public virtual string JsDirectory { get; set; }

        public virtual string CssDirectory { get; set; }

        public virtual bool UseChameJson { get; set; }


        //          //   var sss = ;



    }
}
