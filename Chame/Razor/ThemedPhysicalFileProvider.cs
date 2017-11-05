using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Chame.Razor
{
    public class ThemedPhysicalFileProvider : IFileProvider
    {
        private readonly PhysicalFileProvider _provider;

        public ThemedPhysicalFileProvider(string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _provider = new PhysicalFileProvider(root);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return _provider.GetFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _provider.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _provider.Watch(filter);
        }
    }
}
