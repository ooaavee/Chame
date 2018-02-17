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


            //
            // Create a file provider that will load content from your drive...
            //
            IFileProvider fileProvider = new PhysicalFileProvider(root);
            FileProviders.Add(fileProvider);


            if (controllers != null)
            {
                ViewDefinition[] views = GetViews(env, controllers).ToArray();

                //
                // THEMED VIEWS: 
                // /[theme name]/Views/[controller name]/[page name]/{0}.cshtml
                //
                foreach (ViewDefinition view in views)
                {
                    string template = view.View != null
                        ? $"/{{0}}/{ViewsFolderName}/{view.Controller}/{view.View}/{{{{0}}}}.cshtml"
                        : $"/{{0}}/{ViewsFolderName}/{view.Controller}/{{{{0}}}}.cshtml";

                    ViewLocationTemplates.Add(template);
                }

                //
                // THEMED VIEWS:
                // /[theme name]/Views/Shared/{0}.cshtml
                //
                ViewLocationTemplates.Add($"/{{0}}/{ViewsFolderName}/Shared/{{0}}.cshtml");

                //
                // DEFAULT VIEWS: 
                // /Views/[controller name]/[page name]/{0}.cshtml
                //
                foreach (ViewDefinition view in views)
                {
                    string template = view.View != null
                        ? $"/{ViewsFolderName}/{view.Controller}/{view.View}/{{{{0}}}}.cshtml"
                        : $"/Views/{view.Controller}/{{{{0}}}}.cshtml";

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

        private static IEnumerable<ViewDefinition> GetViews(IHostingEnvironment env, IEnumerable<string> controllers)
        {
            IFileInfo[] views = env.ContentRootFileProvider.GetDirectoryContents(ViewsFolderName).ToArray();

            foreach (string controller in controllers)
            {
                IFileInfo folder = views.FirstOrDefault(x => x.Name == controller && x.IsDirectory);

                if (folder != null)
                {
                    foreach (string view in Directory.GetDirectories(folder.PhysicalPath))
                    {
                        yield return new ViewDefinition {Controller = controller, View = new DirectoryInfo(view).Name};
                    }

                    yield return new ViewDefinition { Controller = controller };
                }
            }
        }

        private sealed class ViewDefinition
        {
            public string Controller { get; set; }
            public string View { get; set; }
        }
    }
}
