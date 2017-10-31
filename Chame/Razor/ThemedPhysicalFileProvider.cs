using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Chame.Razor
{
    public class ThemedPhysicalFileProvider : IFileProvider
    {
        private NullFileProvider _null = new NullFileProvider();

        public ThemedPhysicalFileProvider(string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

//           new PhysicalFileProvider()
            
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return _null.GetFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _null.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _null.Watch(filter);
        }
    }
}
