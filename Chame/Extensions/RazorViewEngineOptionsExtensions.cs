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
        public static void EnableThemes(this RazorViewEngineOptions options, Action<RazorThemeOptions> configureOptions = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            // Create and configure options.
            var themes = new RazorThemeOptions();
            configureOptions?.Invoke(themes);

            // Register a view-location-expander.
            options.ViewLocationExpanders.Add(new ThemedViewLocationExpander(themes));

            // Register assemblies for embedded Razor views.
            if (themes.EmbeddedViewAssemblies.Any())
            {
                var providers = new List<IFileProvider>();
                foreach (Assembly assembly in themes.EmbeddedViewAssemblies)
                {
                    providers.Add(new EmbeddedFileProvider(assembly));
                }
                options.FileProviders.Add(new CompositeFileProvider(providers));
            }

        }
    }
}