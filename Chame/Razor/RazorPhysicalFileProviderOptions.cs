using System;
using System.Collections.Generic;

namespace Chame.Razor
{
    public class RazorPhysicalFileProviderOptions
    {
        /// <summary>
        /// The root directory. This should be an absolute path.
        /// </summary>
        public string Root { get; }

        public IReadOnlyCollection<string> NamedControllers { get; }

        public RazorPhysicalFileProviderOptions(string root, IEnumerable<string> namedControllers = null)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            Root = root;

            if (namedControllers != null)
            {
                NamedControllers = new List<string>(namedControllers);
            }
            else
            {                
                NamedControllers = new List<string>();
            }
        }

    }
}
