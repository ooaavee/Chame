using System;
using System.Runtime.Versioning;
using Chame;
using Chame.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// IApplicationBuilder extension methods
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        private const string DefaultPathTemplate = "chame-loader/{0}";

        public static IApplicationBuilder UseThemes(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            IOptions<ContentLoaderOptions> options = app.ApplicationServices.GetService<IOptions<ContentLoaderOptions>>();
            if (options == null)
            {
                throw new InvalidOperationException(string.Format("Unable to find the required services. Please add all the required services by calling '{0}.{1}' inside the call to 'ConfigureServices(...)' in the application startup code.", nameof(IServiceCollection), "AddThemes"));
            }

            IContentModel cm = options.Value.ContentModel;


            app.UseMiddleware<ContentLoaderMiddleware>(DefaultPathTemplate);

            return app;
        }
    }
}
