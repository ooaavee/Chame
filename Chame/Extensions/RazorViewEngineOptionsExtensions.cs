using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chame;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RazorViewEngineOptionsExtensions
    {
        public static void EnableChame(this RazorViewEngineOptions options, Action<ChameRazorViewEngineOptions> configureOptions = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Create and configure options.
            ChameRazorViewEngineOptions opt = new ChameRazorViewEngineOptions();
            if (configureOptions != null)
            {
                configureOptions(opt);
            }
         
            // Register a view-location-expander.
            options.ViewLocationExpanders.Add(new ChameViewLocationExpander(opt));

            // Register assemblies for embedded Razor views.
            if (opt.EmbeddedViewAssemblies.Any())
            {
                var providers = new List<IFileProvider>();
                foreach (Assembly assembly in opt.EmbeddedViewAssemblies)
                {
                    providers.Add(new EmbeddedFileProvider(assembly));
                }
                options.FileProviders.Add(new CompositeFileProvider(providers));
            }

        }
    }
}