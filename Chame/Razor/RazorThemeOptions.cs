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

            if (options.NamedControllers.Any())
            {
                Tuple<string, string>[] viewFolders = GetViewFolders(env, options.NamedControllers).ToArray();

                // THEMED VIEWS: 
                // [theme name]/Views/[controller name]/[page name]/{0}.cshtml
                foreach (Tuple<string, string> viewFolder in viewFolders)
                {
                    string controller = viewFolder.Item1;
                    string page = viewFolder.Item2;
                    string template;

                    if (page != null)
                    {
                        template = "{0}/Views/" + controller + "/" + page + "/{{0}}.cshtml";
                    }
                    else
                    {
                        template = "{0}/Views/" + controller + "/{{0}}.cshtml";
                    }

                    ViewLocationTemplates.Add(template);
                }

                // THEMED VIEWS:
                // [theme name]/Views/Shared/{0}.cshtml
                ViewLocationTemplates.Add("{0}/Views/Shared/{{0}}.cshtml");

                // DEFAULT VIEWS: 
                // Views /[controller name]/[page name]/{0}.cshtml
                foreach (Tuple<string, string> viewFolder in viewFolders)
                {
                    string controller = viewFolder.Item1;
                    string page = viewFolder.Item2;
                    string template;

                    if (page != null)
                    {
                        template = "Views/" + controller + "/" + page + "/{{0}}.cshtml";
                    }
                    else
                    {
                        template = "Views/" + controller + "/{{0}}.cshtml";
                    }

                    ViewLocationTemplates.Add(template);
                }
            }
            else
            {
                ViewLocationTemplates.Add("{0}/Views/{{1}}/{{0}}.cshtml");
                ViewLocationTemplates.Add("{0}/Views/Shared/{{0}}.cshtml");
                ViewLocationTemplates.Add("{0}/Views/{{0}}.cshtml");
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
            IFileInfo[] viewsSubFolders = env.ContentRootFileProvider.GetDirectoryContents("Views").ToArray();

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
