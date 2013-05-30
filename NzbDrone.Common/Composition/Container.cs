using System;
using System.Collections.Generic;
using System.Linq;
using TinyIoC;

namespace NzbDrone.Common.Composition
{
    public class Container : IContainer
    {
        private readonly TinyIoCContainer _container;
        private readonly List<Type> _loadedTypes;

        public Container(TinyIoCContainer container, List<Type> loadedTypes)
        {
            _container = container;
            _loadedTypes = loadedTypes;
            _container.Register<IContainer>(this);
        }

        public void Register<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            _container.Register<TService, TImplementation>();
        }

        public TinyIoCContainer TinyContainer { get { return _container; } }

        public void Register<T>(T instance) where T : class
        {
            _container.Register<T>(instance);
        }

        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        public void Register(Type serviceType, Type implementationType)
        {
            _container.Register(serviceType, implementationType);
        }

        public void Register<TService>(Func<IContainer, TService> factory) where TService : class
        {
            _container.Register((c, n) =>
                {
                    return factory(this);
                });
        }

        public void RegisterSingleton<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            _container.Register<TService, TImplementation>().AsSingleton();
        }

        public void RegisterSingleton<T>() where T : class
        {
            _container.Register<T, T>().AsSingleton();
        }

        public void RegisterSingleton(Type service, Type implementation)
        {
            _container.Register(service, implementation).AsSingleton();
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return _container.ResolveAll<T>();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return _container.ResolveAll(type);
        }

        public void Register(Type registrationType, object instance)
        {
            _container.Register(registrationType, instance);
        }

        public void RegisterAllAsSingleton(Type registrationType, IEnumerable<Type> implementationList)
        {
            _container.RegisterMultiple(registrationType, implementationList).AsSingleton();
        }

        public bool IsTypeRegistered(Type type)
        {
            return _container.CanResolve(type);
        }

        public IEnumerable<Type> GetImplementations(Type contractType)
        {
            return _loadedTypes
                .Where(implementation =>
                       contractType.IsAssignableFrom(implementation) &&
                       !implementation.IsInterface &&
                       !implementation.IsAbstract
                );
        }
    }
}