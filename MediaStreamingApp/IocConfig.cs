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
            di.AddSingletone<IOuth2CodeSynchronizer, InProcessOuth2CodeSynchronizer>();

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
                var accessToken = config["MediaStorage:GoogleDrive:AccessToken"];
                var refreshoken = config["MediaStorage:GoogleDrive:RefreshToken"];
                var appName = config["MediaStorage:GoogleDrive:AppName"];
                var clientId = config["MediaStorage:GoogleDrive:ClientId"];
                var clientSecret = config["MediaStorage:GoogleDrive:ClientSecret"];
                string folderPath = config.GetValue<string>("MediaStorage:GoogleDrive:DataStoreFolder", string.Empty);
                bool isFullPath = config.GetValue<bool>("DataStoreFolderIsFullPath", true);

                if(string.IsNullOrEmpty(accessToken))
                {
                    var gdriveStorage =  GoogleDriveStorageFactory.Create(appName, clientId, clientSecret);
                    if(!string.IsNullOrEmpty(folderPath))
                    {
                        gdriveStorage.DataStoreFolder = folderPath;
                        gdriveStorage.IsFullPath = isFullPath;
                    }

                    //gdriveStorage.AuthorizeRedirectUrl = config.GetValue<string>("MediaStorage:GoogleDrive:CodeReceiverUrl", string.Empty);
                    return gdriveStorage;
                }
                else
                {
                    var gdriveStorage = GoogleDriveStorageFactory.CreateUsingAccessToken(appName, clientId, clientSecret, accessToken, refreshoken);
                    if(!string.IsNullOrEmpty(folderPath))
                    {
                        gdriveStorage.DataStoreFolder = folderPath;
                        gdriveStorage.IsFullPath = isFullPath;
                    }
                    return gdriveStorage;
                }
            });
        }
    }
}
