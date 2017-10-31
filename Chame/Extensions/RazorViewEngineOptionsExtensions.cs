using System;
using Chame.Razor;
using Microsoft.AspNetCore.Mvc.Razor;

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
            if (configureOptions != null)
            {
                configureOptions(themes);
            }

            // Register a view-location-expander.
            IViewLocationExpander expander = new ThemedViewLocationExpander(themes);
            options.ViewLocationExpanders.Add(expander);            
        }
    }
}