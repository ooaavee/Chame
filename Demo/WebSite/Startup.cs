using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chame;
using Chame.ContentLoaders;
using Chame.Razor;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            // Resolve an absolute path of the'Root' directory under the 'WebSiteContent' project.
            string contentRoot = new DirectoryInfo(_env.ContentRootPath)
                .Parent
                .GetDirectories()
                .First(x => x.Name == "WebSiteContent")
                .GetDirectories()
                .First(x => x.Name == "Root")
                .FullName;

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
                        themes.WithPhysicalFileProvider(contentRoot, _env, new[] {"Demo"});
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

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
