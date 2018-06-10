using System;
using MediaStorage.Common.DI;
using MediaStorage.Data.Media.Context;

namespace MediaStorage.Data.Media
{
    public static class IocConfig
    {
        public static void RegisterMediaDataContextServices(this IAbstractDependencyInjector di)
        {
            // Configure service types.
            di.AddPerThread<IMediaDataContext, MediaDataContext>();
        }
    }
}
