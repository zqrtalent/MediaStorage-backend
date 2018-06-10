using System;
using MediaStorage.Common.DI;
using MediaStorage.Data.Streaming.Context;

namespace MediaStorage.Data.Media
{
    public static class IocConfig
    {
        public static void RegisterStreamingDataContextServices(this IAbstractDependencyInjector di)
        {
            // Configure service types.
            di.AddPerThread<IStreamingDataContext, StreamingDataContext>();
        }
    }
}
