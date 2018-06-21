using System;
using MediaStorage.Common.DI;
using MediaStorage.IO;
using MediaStorage.IO.FileStream;
using MediaStorage.IO.GoogleDrive;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MediaStreamingApp
{
    public static class IocConfig
    {
        public static void RegisterStorageServices(this IAbstractDependencyInjector di, IHostingEnvironment env)
        {
            // Configure service types.
            di.AddPerThread<ILocalStorage>(provider => {
                var config = provider.Resolve<IConfiguration>();
                var mlpath = config["MediaStorage:Local:MediaLibraryPath"];
                var tempMlPath = config["MediaStorage:Local:TempLibraryPath"];

                if(mlpath.StartsWith("~"))
                    mlpath = env.ContentRootPath + mlpath.Substring(1);
                if(tempMlPath.StartsWith("~"))
                    tempMlPath = env.ContentRootPath + tempMlPath.Substring(1);
                return new LocalStorage(mlpath);
            });

            di.AddPerThread<IStorage>(provider => {
                var config = provider.Resolve<IConfiguration>();
                Console.WriteLine($"{config["MediaStorage:GoogleDrive:AppName"]} - {config["MediaStorage:GoogleDrive:ClientId"]} - {config["MediaStorage:GoogleDrive:ClientSecret"]}");
                return GoogleDriveStorageFactory.Create(
                    config["MediaStorage:GoogleDrive:AppName"], 
                    config["MediaStorage:GoogleDrive:ClientId"], 
                    config["MediaStorage:GoogleDrive:ClientSecret"]);
            });
        }
    }
}
