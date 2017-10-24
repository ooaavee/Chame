using System;
using Chame.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// IApplicationBuilder extension methods
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        private const string DefaultPathTemplate = "/chame-loader/{0}";

        public static IApplicationBuilder UseThemes(this IApplicationBuilder app, string pathTemplate = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            app.UseMiddleware<ContentLoaderMiddleware>(pathTemplate ?? DefaultPathTemplate);

            return app;
        }
    }
}
