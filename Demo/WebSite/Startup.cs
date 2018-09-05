using Chame;
using Chame.ContentLoaders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebSite
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // wwwroot/ChameContent is our content root
            string contentRoot = Path.Combine(_env.WebRootPath, "ChameContent");

            // Add content loaders.
            services.AddContentLoader(x =>
            {                              
            }).AddFileSystemLoader(options => { options.Root = contentRoot; });

            // Add theme resolver.
            services.AddSingleton<IThemeResolver, DemoThemeResolver>();

            services.AddSingleton<IContentNotFoundCallback, DemoContentNotFoundCallback>();

            // Add MVC.
            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    options.EnableThemes(themes =>
                    {
                        themes.ViewLocationTemplates.Add("/Views/ChameContent/{0}/{{1}}/{{0}}.cshtml");
                        themes.ViewLocationTemplates.Add("/Views/ChameContent/{0}/Shared/{{0}}.cshtml");
                        themes.ViewLocationTemplates.Add("/Views/ChameContent/{0}/{{0}}.cshtml");

                        themes.WithViewLocationExpander();
                    });
                });

            // Cookie authentication is needed for demo purposes only: authenticated users have a different theme in this demo!
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "Chame";
                });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            // Use content loader middleware.
            app.UseContentLoader();

            app.UseMiddleware<HideChameContentMiddleware>();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }


        public class HideChameContentMiddleware
        {
            private readonly RequestDelegate _next;

            public HideChameContentMiddleware(RequestDelegate next)
            {
                _next = next;                
            }

            public async Task Invoke(HttpContext context)
            {
                if (context.Request.Method == HttpMethods.Get)
                {
                    if (context.Request.Path.StartsWithSegments("/ChameContent", StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }
                }
                await _next(context);
            }
        }

    }
}
