using System;
using Microsoft.Extensions.DependencyInjection;
using MediaStorage.Common.DI.Microsoft;

namespace MediaStorage.Common.DI.Extensions
{
    public static class MicrosoftDependencyExtensions
    {
        public static IAbstractDependencyInjector UseMicrosoftDI(this IServiceCollection container, IServiceProvider provider)
        {
            return new MicrosoftDependencyInjector(container, provider);
        }

        public static IAbstractDependencyInjector UseMicrosoftDI(IServiceProvider provider)
        {
            return new MicrosoftDependencyInjector(provider);
        }
    }
}
