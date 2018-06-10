using System;

namespace MediaStorage.Common.DI
{
    public interface IAbstractDependencyInjector : IDisposable
    {
        void AddTransient<TService, TImplementation>(string name = null)
            where TService : class
            where TImplementation : class, TService;

        void AddTransient<TService>(Func<IAbstractDependencyInjector, object> implementationFactory)
            where TService : class;

        void AddPerThread<TService, TImplementation>(string name = null)
            where TService : class
            where TImplementation : class, TService;

        void AddPerThread<TService>(Func<IAbstractDependencyInjector, object> implementationFactory)
            where TService : class;

        void AddSingletone<TService, TImplementation>(string name = null)
            where TService : class
            where TImplementation : class, TService;

        void AddSingletone<TService>(Func<IAbstractDependencyInjector, object> implementationFactory) 
            where TService : class;

        void AddSingletone<TService, TImplementation>(TImplementation instance, string name = null)  
            where TService : class
            where TImplementation : class, TService;

        void AddSingletone<TService>(TService instance, string name = null)  
            where TService : class;

        IServiceProvider Build();

        T Resolve<T>(string name = null) where T : class;
    }
}