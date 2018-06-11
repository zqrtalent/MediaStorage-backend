using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediaStorage.Common.DI;
using MediaStorage.Common.DI.Extensions;

using MediaStorage.Core.Services;
using MediaStorage.Data.Media;
using MediaStorage.Data.Streaming;
using MediaStorage.StreamingApi.Auth;
using Microsoft.AspNetCore.Authentication;
using MediaStorage.Settings;

/*
ASPNETCORE_ENVIRONMENT=Development dotnet run
localhost:5000/api/v1/audiopackets/offset?mediaId=weerwe&offset=0&numpackets=1
 */

namespace MediaStorage.StreamingApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IServiceProvider _serviceProvider;
        private static IAbstractDependencyInjector _di;

        public Startup(IConfiguration configuration, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _hostingEnvironment = env;
            _serviceProvider = serviceProvider;

            /*
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            ContentRootPath = env.ContentRootPath;
             */
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("MyAuth")
            .AddScheme<MyAuthSchemeOptions, MyAuthenticationHandler>("MyAuth", options => {
                var authSettings = _di.Resolve<IOptions<AuthSettings>>().Value;
                options.Scheme = "MyAuth";
                options.AccessTokenKeyHeaderName = authSettings.AccessTokenHeaderName;
                options.SessionInfoContextKeyName = authSettings.SessionInfoContextKeyName;
                options.Expiration = new TimeSpan((authSettings.ExpirationInMin - authSettings.ExpirationInMin%60)  / 60, 
                     authSettings.ExpirationInMin%60, 0);
            });

            services.AddMvc();
            services.AddMemoryCache();

            // Configure settings.
            services.AddOptions();
            services.Configure<GoogleDriveSettings>(_configuration.GetSection("MediaStorage:GoogleDrive"));
            services.Configure<LocalDriveSettings>(_configuration.GetSection("MediaStorage:Local"));
            services.Configure<AuthSettings>(_configuration.GetSection("Auth"));

            // Configure dependency abstructor.
            _di = services.UseMicrosoftDI(_serviceProvider);
            _di.AddSingletone(_configuration);
            _di.AddSingletone(_di);
            _di.AddPerThread<AuthenticationHandler<MyAuthSchemeOptions>, MyAuthenticationHandler>();

            // Start registering services.
            _di.RegisterStorageServices(_hostingEnvironment);
            _di.RegisterMediaDataContextServices();
            _di.RegisterStreamingDataContextServices();
            _di.RegisterCoreServices();
            // End registering services.

            return _di.Build();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggingFactory loggingFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}