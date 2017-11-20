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
        /// <param name="options"></param>
        /// <param name="env">web hosting environment</param>
        public void WithPhysicalFileProvider(RazorPhysicalFileProviderOptions options, IHostingEnvironment env)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            IFileProvider provider = new ThemedPhysicalFileProvider(options.Root);
            FileProviders.Add(provider);

            List<string> templates = new List<string>();

            if (options.NamedControllers.Any())
            {
                Tuple<string, string>[] viewFolders = GetViewFolders(env, options.NamedControllers).ToArray();

                foreach (Tuple<string, string> viewFolder in viewFolders)
                {
                    string controller = viewFolder.Item1;
                    string page = viewFolder.Item2;
                    string template = "{0}/Views/" + controller + "/" + page + "/{{0}}.cshtml";
                    templates.Add(template);
                }

                foreach (Tuple<string, string> viewFolder in viewFolders)
                {
                    string controller = viewFolder.Item1;
                    string page = viewFolder.Item2;
                    string template = "Views/" + controller + "/" + page + "/{{0}}.cshtml";
                    templates.Add(template);
                }

                templates.Add("{0}/Views/Shared/{{0}}.cshtml");
            }
            else
            {
                templates.Add("{0}/Views/{{1}}/{{0}}.cshtml");
                templates.Add("{0}/Views/Shared/{{0}}.cshtml");
                templates.Add("{0}/Views/{{0}}.cshtml");
            }

            foreach (var template in templates)
            {
                ViewLocationTemplates.Add(template);
            }
        }

        /// <summary>
        /// Enables view location expander.
        /// </summary>
        public void WithViewLocationExpander()
        {
            IViewLocationExpander expander = new ThemedViewLocationExpander(this);
            ViewLocationExpanders.Add(expander);
        }

        private static IEnumerable<Tuple<string, string>> GetViewFolders(IHostingEnvironment env, IEnumerable<string> controllers)
        {
            IDirectoryContents viewsSubFolders = env.ContentRootFileProvider.GetDirectoryContents("Views");

            foreach (string controller in controllers)
            {
                IFileInfo controllerFolder = viewsSubFolders.ToArray().FirstOrDefault(x => x.Name == controller && x.IsDirectory);

                if (controllerFolder != null)
                {
                    string[] pages = Directory.GetDirectories(controllerFolder.PhysicalPath);

                    foreach (string page in pages)
                    {
                        DirectoryInfo pageInfo = new DirectoryInfo(page);

                        yield return new Tuple<string, string>(controller, pageInfo.Name);
                    }
                }
            }
        }

    }
}
