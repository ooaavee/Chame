using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace Chame.Razor
{
    public class RazorThemeOptions
    {
        public RazorThemeOptions()
        {
            //
            // TODO: Nämä pois
            //
            ViewLocationTemplates.Add("Views/Themes/{0}/{{1}}/{{0}}.cshtml");
            ViewLocationTemplates.Add("Views/Themes/{0}/Shared/{{0}}.cshtml");
            ViewLocationTemplates.Add("Views/Themes/{0}/{{0}}.cshtml");
        }

        /// <summary>
        /// Gets the sequence of Microsoft.Extensions.FileProviders.IFileProvider instances used by Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to locate Razor files.
        /// </summary>
        public IList<IFileProvider> FileProviders { get; } = new List<IFileProvider>();

        /// <summary>
        /// View location templates.
        /// </summary>
        public IList<string> ViewLocationTemplates { get; } = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root">The root directory. This should be an absolute path.</param>
        public void WithPhysicalFileProvider(string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            FileProviders.Add(new ThemedPhysicalFileProvider(root));

            ViewLocationTemplates.Add("{0}/Views/{{1}}/{{0}}.cshtml");
            ViewLocationTemplates.Add("{0}/Views/Shared/{{0}}.cshtml");
            ViewLocationTemplates.Add("{0}/Views/{{0}}.cshtml");
        }

    }

}
