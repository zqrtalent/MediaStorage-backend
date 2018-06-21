using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MediaStreamingApp.Models;
using MediaStreamingApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

using MediaStorage.IO;
using MediaStorage.IO.GoogleDrive;
using MediaStorage.IO.FileStream;
using MediaStorage.Common.DI;
using MediaStorage.Common.DI.Extensions;
using MediaStorage.Data.Media;
using MediaStorage.Core.Services;
using Newtonsoft.Json.Serialization;
using MediaStorage.Data.WebApp.Entities;
using MediaStorage.Data.WebApp.Context;
using Microsoft.AspNetCore.Http;

/*
ASPNETCORE_ENVIRONMENT=Development dotnet run
localhost:5000/api/v1/audiopackets/offset?mediaId=weerwe&offset=0&numpackets=1
 */

namespace MediaStreamingApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IServiceProvider _serviceProvider;
        private static IAbstractDependencyInjector _di;
        private readonly string _contentRootPath;
        
        public Startup(IConfiguration configuration, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            // var builder = new ConfigurationBuilder()
            //     .SetBasePath(env.ContentRootPath)
            //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //     .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            // if (env.IsDevelopment())
            // {
            //     // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
            //     builder.AddUserSecrets<Startup>();
            // }

            // builder.AddEnvironmentVariables();
            // _configuration = builder.Build();
            _configuration = configuration;
            _contentRootPath = env.ContentRootPath;
            _hostingEnvironment = env;
            _serviceProvider = serviceProvider;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddScoped<WebAppDataContext>(provider => new WebAppDataContext(provider.GetService<IConfiguration>()));
            services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<WebAppDataContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => { 
                options.LoginPath = "/Account/LogIn"; 
                // options.Cookie.Name = ".AuthCookie";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
             
            services.AddMvc()
            .AddJsonOptions(options => {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver(); // Without this contract property names will change useing camelCase.
            });
            
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // Configure dependency abstructor.
            _di = services.UseMicrosoftDI(_serviceProvider);
            _di.AddSingletone(_configuration);
            //services.AddSingleton(typeof(IConfiguration), _configuration);
            _di.AddSingletone(_di);

            // Start registering services.
            _di.RegisterStorageServices(_hostingEnvironment);
            _di.RegisterMediaDataContextServices();
            _di.RegisterCoreServices();
            // End registering services.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddFile("Logs/errorlog-{Date}.txt");
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}