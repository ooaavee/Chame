using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chame;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using WebSite.Services;

namespace WebSite
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddChame(options => { options.ThemeResolver = new DemoThemeResolver(); })
                .AddFileSystemLoader(options =>
                {
                });


            // Add MVC.
            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    options.EnableThemes(themes =>
                    {
                        themes.EmbeddedViewAssemblies.Add(typeof(WebSite.Themes.A.Info).Assembly);
                        themes.EmbeddedViewAssemblies.Add(typeof(WebSite.Themes.B.Info).Assembly);
                    });
                });



            services.AddTransient<DemoService, DemoService>();

            // Cookie authentication is needed for demo purposes only: authenticated users have a 
            // different theme in these samples.
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "Chame";
                });



        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
            }

            app.UseAuthentication();

            app.UseChame();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
