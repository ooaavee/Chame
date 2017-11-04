using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Chame.Razor
{
    public class ThemedPhysicalFileProvider : IFileProvider
    {
        private readonly PhysicalFileProvider _physicalFile;

        public ThemedPhysicalFileProvider(string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _physicalFile = new PhysicalFileProvider(root);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return Provider.GetFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return Provider.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return Provider.Watch(filter);
        }

        /// <summary>
        /// Gets the underlying file provider.
        /// </summary>
        private IFileProvider Provider => _physicalFile;
    }
}
