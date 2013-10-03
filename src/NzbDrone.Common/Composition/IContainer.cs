using System;
using System.Collections.Generic;
using TinyIoC;

namespace NzbDrone.Common.Composition
{
    public interface IContainer
    {
        TinyIoCContainer TinyContainer { get; }
        void Register<T>(T instance) where T : class;

        void Register<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class;
        T Resolve<T>() where T : class;
        object Resolve(Type type);
        void Register(Type serviceType, Type implementationType);
        void Register<TService>(Func<IContainer, TService> factory) where TService : class;
        void RegisterSingleton<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class;
        void RegisterSingleton<T>() where T : class;
        void RegisterSingleton(Type service, Type implementation);
        IEnumerable<T> ResolveAll<T>() where T : class;
        IEnumerable<object> ResolveAll(Type type);
        void Register(Type registrationType, object instance);
        void RegisterAllAsSingleton(Type registrationType, IEnumerable<Type> implementationList);
        bool IsTypeRegistered(Type type);

        IEnumerable<Type> GetImplementations(Type contractType);
    }
}