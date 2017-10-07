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
        public static void EnableChame(this RazorViewEngineOptions options, Action<ChameRazorOptions> configureOptions = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var chame = new ChameRazorOptions();
            configureOptions?.Invoke(chame);

            // Register a view-location-expander.
            options.ViewLocationExpanders.Add(new ChameViewLocationExpander());

            // Register assemblies for embedded Razor views.
            if (chame.EmbeddedViewAssemblies.Any())
            {
                var providers = new List<IFileProvider>();
                foreach (Assembly assembly in chame.EmbeddedViewAssemblies)
                {
                    providers.Add(new EmbeddedFileProvider(assembly));
                }
                options.FileProviders.Add(new CompositeFileProvider(providers));
            }

        }
    }
}