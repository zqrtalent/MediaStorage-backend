using System;
using MediaStorage.Common.DI;

namespace MediaStorage.Core.Services
{
    public static class IocConfig
    {
        public static void RegisterCoreServices(this IAbstractDependencyInjector di)
        {
            // Configure service types.
            di.AddPerThread<IMediaLibraryService, MediaLibraryService>();
            di.AddPerThread<IMediaStreamingService, MediaStreamingService>();
            di.AddPerThread<IMediaUploadService, MediaUploadService>();
            di.AddPerThread<IImageResourceService, ImageResourceService>();
            di.AddPerThread<IStreamingUserService, StreamingUserService>();
            di.AddPerThread<IStreamingUserSessionService, StreamingUserSessionService>();
            di.AddPerThread<IStreamingUserCacheService, StreamingUserCacheService>();
        }
    }
}
