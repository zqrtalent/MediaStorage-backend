using System;
using MediaStorage.Common.DI;
using MediaStorage.Data.WebApp.Context;

namespace MediaStorage.Data.WebApp
{
    public static class IocConfig
    {
        public static void RegisterWebAppDataContextServices(this IAbstractDependencyInjector di)
        {
            // Configure service types.
            di.AddPerThread<IWebAppDataContext, WebAppDataContext>();
        }
    }
}
