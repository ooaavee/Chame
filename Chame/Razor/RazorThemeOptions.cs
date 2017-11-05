using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;

namespace Chame.Razor
{
    public class RazorThemeOptions
    {
        /// <summary>
        /// Gets the sequence of <see cref="IFileProvider"/> instances used by <see cref="RazorViewEngine"/> to locate Razor files.
        /// </summary>
        public IList<IFileProvider> FileProviders { get; } = new List<IFileProvider>();

        /// <summary>
        /// View location templates
        /// </summary>
        public IList<string> ViewLocationTemplates { get; } = new List<string>();

        /// <summary>
        /// View location expanders
        /// </summary>
        public IList<IViewLocationExpander> ViewLocationExpanders { get; } = new List<IViewLocationExpander>();

        /// <summary>
        /// Enables the physical file provider.
        /// </summary>
        /// <param name="root">The root directory. This should be an absolute path.</param>
        public void WithPhysicalFileProvider(string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            IFileProvider provider = new ThemedPhysicalFileProvider(root);
            FileProviders.Add(provider);
           
            ViewLocationTemplates.Add("{0}/Views/{{1}}/{{0}}.cshtml");
            ViewLocationTemplates.Add("{0}/Views/Shared/{{0}}.cshtml");
            ViewLocationTemplates.Add("{0}/Views/{{0}}.cshtml");
        }

        /// <summary>
        /// Enables view location expander.
        /// </summary>
        public void WithViewLocationExpander()
        {
            IViewLocationExpander expander = new ThemedViewLocationExpander(this);
            ViewLocationExpanders.Add(expander);
        }
    }

}
