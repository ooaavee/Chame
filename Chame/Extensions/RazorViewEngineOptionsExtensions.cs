using System;
using Chame.Razor;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RazorViewEngineOptionsExtensions
    {
        public static void EnableThemes(this RazorViewEngineOptions options, Action<RazorThemeOptions> setupAction = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            // create and configure options
            var o = new RazorThemeOptions();
            setupAction?.Invoke(o);

            // register view-location expanders
            foreach (IViewLocationExpander item in o.ViewLocationExpanders)
            {
                options.ViewLocationExpanders.Add(item);
            }

            // register file providers
            foreach (IFileProvider item in o.FileProviders)
            {
                options.FileProviders.Add(item);
            }
        }
    }
}