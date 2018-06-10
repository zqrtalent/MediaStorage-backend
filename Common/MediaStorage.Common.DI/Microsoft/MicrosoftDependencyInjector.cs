using System;
using Microsoft.Extensions.DependencyInjection;

namespace MediaStorage.Common.DI.Microsoft
{
   
   /* public class Scoped<T> : IDisposable
    {
        private readonly IServiceScope _scope;

        public Scoped(IServiceProvider provider)
        {
            var factory = provider.GetRequiredService<IServiceScopeFactory>();

            _scope = factory.CreateScope();

            Value = _scope.ServiceProvider.GetRequiredService<T>();
        }

        public T Value { get; }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }*/

    /*
     services.AddScoped(typeof(IMediaLibraryService), 
                serviceProvider => new MediaLibraryService(
                    (MediaDataContext)serviceProvider.GetService(typeof(MediaDataContext))));
     */

    internal class MicrosoftDependencyInjector : IAbstractDependencyInjector, IDisposable
    {
        private readonly IServiceCollection _collection;
        private ServiceProvider _provider;
        private IServiceProvider _serviceProvider;

        public MicrosoftDependencyInjector(IServiceProvider provider = null)
        {
        #if DEBUG
            Console.WriteLine($"{nameof(MicrosoftDependencyInjector)} -> new");
        #endif

            // Creates service collection.
            _collection = new ServiceCollection();
            _serviceProvider = provider;
        }

        ~MicrosoftDependencyInjector()
        {
            Dispose(false);
        }
        
        public MicrosoftDependencyInjector(IServiceCollection collection, IServiceProvider provider = null)
        {
            _collection = collection;
            _serviceProvider = provider;
        }

        public void AddTransient<TService, TImplementation>(string name = null)
            where TService : class
            where TImplementation : class, TService
        {
            if(string.IsNullOrEmpty(name))
                _collection.AddTransient<TService, TImplementation>();
            else
            {
                throw new NotSupportedException($"Named injection is not supported!");
            }
        }

        public void AddTransient<TService>(Func<IAbstractDependencyInjector, object> implementationFactory)
            where TService : class
        {
            var injector = this;
            _collection.AddTransient<TService>(provider => (TService)implementationFactory.Invoke(injector));
        }

         public void AddPerThread<TService, TImplementation>(string name = null)  
            where TService : class
            where TImplementation : class, TService
        {
            if(string.IsNullOrEmpty(name))
            {
                _collection.AddScoped<TService, TImplementation>();
            }
            else
            {
                throw new NotSupportedException($"Named injection is not supported!");
            }
        }

         public void AddPerThread<TService>(Func<IAbstractDependencyInjector, object> implementationFactory)
            where TService : class
        {
            var injector = this;
            _collection.AddScoped<TService>(provider => (TService)implementationFactory.Invoke(injector));
        }

        public void AddSingletone<TService, TImplementation>(string name = null)  
            where TService : class
            where TImplementation : class, TService
        {
            if(string.IsNullOrEmpty(name))
            {
                _collection.AddSingleton<TService, TImplementation>();
            }
            else
            {
                throw new NotSupportedException($"Named injection is not supported!");
            }
        }

        public void AddSingletone<TService>(Func<IAbstractDependencyInjector, object> implementationFactory)
            where TService : class
        {
            var injector = this;
            _collection.AddSingleton<TService>(provider => (TService)implementationFactory.Invoke(injector));
        }

        public void AddSingletone<TService, TImplementation>(TImplementation instance, string name = null)  
            where TService : class
            where TImplementation : class, TService
        {
            if(string.IsNullOrEmpty(name))
            {
                _collection.AddSingleton(typeof(TService), instance);
            }
            else
            {
                throw new NotSupportedException($"Named injection is not supported!");
            }
        }

        public void AddSingletone<TService>(TService instance, string name = null)  
            where TService : class
        {
            if(string.IsNullOrEmpty(name))
            {
                _collection.AddSingleton(typeof(TService), instance);
            }
            else
            {
                throw new NotSupportedException($"Named injection is not supported!");
            }
        }

        public IServiceProvider Build()
        {
            _provider = _collection.BuildServiceProvider();
            _serviceProvider = _provider;
            return _provider;

            // if(_provider == null){
            //     _provider = _collection.BuildServiceProvider();
            //     _serviceProvider = _provider;
            // }
        }

        public T Resolve<T>(string name = null) 
            where T : class
        {
            return _serviceProvider.GetRequiredService<T>();
            //return _provider.GetService<T>();
        }

        #region Dispose managed/unmanaged resources.
        // Flag: Has Dispose already been called?
        bool disposed = false;
        
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

        #if DEBUG
            Console.WriteLine($"{nameof(MicrosoftDependencyInjector)} -> Dispose");
        #endif

            if (disposing) 
            {
                // Free any other managed objects here.
                if(_provider != null)
                {
                    _provider.Dispose();
                    _provider = null;
                }
            }

            // Free any unmanaged objects here.
            disposed = true;
        }
        #endregion
    }
}
