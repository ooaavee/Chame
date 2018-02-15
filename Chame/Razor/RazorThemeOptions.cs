using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;

namespace Chame.Razor
{
    public class RazorThemeOptions
    {
        private const string ViewsFolderName = "Views";

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
        public void WithPhysicalFileProvider(string root, IHostingEnvironment env, IEnumerable<string> controllers = null)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            IFileProvider fileProvider = new ThemedPhysicalFileProvider(root);
            FileProviders.Add(fileProvider);

            if (controllers != null)
            {
                Tuple<string, string>[] viewFolders = GetViewFolders(env, controllers).ToArray();

                // THEMED VIEWS: 
                // /[theme name]/Views/[controller name]/[page name]/{0}.cshtml
                foreach (Tuple<string, string> viewFolder in viewFolders)
                {
                    string controller = viewFolder.Item1;
                    string page = viewFolder.Item2;
                    string template = page != null ? $"/{{0}}/{ViewsFolderName}/{controller}/{page}/{{{{0}}}}.cshtml" : $"/{{0}}/{ViewsFolderName}/{controller}/{{{{0}}}}.cshtml";

                    ViewLocationTemplates.Add(template);
                }

                // THEMED VIEWS:
                // /[theme name]/Views/Shared/{0}.cshtml
                ViewLocationTemplates.Add($"/{{0}}/{ViewsFolderName}/Shared/{{0}}.cshtml");

                // DEFAULT VIEWS: 
                // /Views/[controller name]/[page name]/{0}.cshtml
                foreach (Tuple<string, string> viewFolder in viewFolders)
                {
                    string controller = viewFolder.Item1;
                    string page = viewFolder.Item2;
                    string template = page != null ? $"/{ViewsFolderName}/{controller}/{page}/{{{{0}}}}.cshtml" : $"/Views/{controller}/{{{{0}}}}.cshtml";

                    ViewLocationTemplates.Add(template);
                }
            }
            else
            {
                ViewLocationTemplates.Add($"/{{0}}/{ViewsFolderName}/{{1}}/{{0}}.cshtml");
                ViewLocationTemplates.Add($"/{{0}}/{ViewsFolderName}/Shared/{{0}}.cshtml");
                ViewLocationTemplates.Add($"/{{0}}/{ViewsFolderName}/{{0}}.cshtml");
            }
        }

        /// <summary>
        /// Enables view location expander.
        /// </summary>
        public void WithViewLocationExpander()
        {
            var expander = new ThemedViewLocationExpander(ViewLocationTemplates);

            ViewLocationExpanders.Add(expander);
        }

        private static IEnumerable<Tuple<string, string>> GetViewFolders(IHostingEnvironment env, IEnumerable<string> controllers)
        {
            IFileInfo[] viewsSubFolders = env.ContentRootFileProvider.GetDirectoryContents(ViewsFolderName).ToArray();

            foreach (string controller in controllers)
            {
                IFileInfo controllerFolder = viewsSubFolders.FirstOrDefault(x => x.Name == controller && x.IsDirectory);

                if (controllerFolder != null)
                {
                    foreach (string page in Directory.GetDirectories(controllerFolder.PhysicalPath))
                    {
                        yield return new Tuple<string, string>(controller, new DirectoryInfo(page).Name);
                    }

                    yield return new Tuple<string, string>(controller, null);
                }
            }
        }
    }
}
