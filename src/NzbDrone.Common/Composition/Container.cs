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

        public void Register<T>(T instance)
            where T : class
        {
            _container.Register<T>(instance);
        }

        public T Resolve<T>()
            where T : class
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

        public void Register<TService>(Func<IContainer, TService> factory)
            where TService : class
        {
            _container.Register((c, n) => factory(this));
        }

        public void RegisterSingleton(Type service, Type implementation)
        {
            var factory = CreateSingletonImplementationFactory(implementation);

            // For Resolve and ResolveAll
            _container.Register(service, factory);

            // For ctor(IEnumerable<T>)
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(service);
            _container.Register(enumerableType, (c, p) =>
            {
                var instance = factory(c, p);
                var result = Array.CreateInstance(service, 1);
                result.SetValue(instance, 0);
                return result;
            });
        }

        public IEnumerable<T> ResolveAll<T>()
            where T : class
        {
            return _container.ResolveAll<T>();
        }

        public void RegisterAllAsSingleton(Type service, IEnumerable<Type> implementationList)
        {
            foreach (var implementation in implementationList)
            {
                var factory = CreateSingletonImplementationFactory(implementation);

                // For ResolveAll and ctor(IEnumerable<T>)
                _container.Register(service, factory, implementation.FullName);
            }
        }

        private Func<TinyIoCContainer, NamedParameterOverloads, object> CreateSingletonImplementationFactory(Type implementation)
        {
            const string singleImplPrefix = "singleImpl_";

            _container.Register(implementation, implementation, singleImplPrefix + implementation.FullName).AsSingleton();

            return (c, p) => _container.Resolve(implementation, singleImplPrefix + implementation.FullName);
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
                       !implementation.IsAbstract);
        }
    }
}
