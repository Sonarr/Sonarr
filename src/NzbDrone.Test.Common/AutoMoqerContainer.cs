using System;
using System.Collections.Generic;
using NzbDrone.Common.Composition;

namespace NzbDrone.Test.Common
{
    public class AutoMoqerContainer : IContainer
    {
        private readonly AutoMoq.AutoMoqer _autoMoqer;

        public AutoMoqerContainer(AutoMoq.AutoMoqer autoMoqer)
        {
            _autoMoqer = autoMoqer;
        }

        public IEnumerable<Type> GetImplementations(Type contractType)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeRegistered(Type type)
        {
            throw new NotImplementedException();
        }

        public void Register<T>(T instance) where T : class
        {
            throw new NotImplementedException();
        }

        public void Register(Type serviceType, Type implementationType)
        {
            throw new NotImplementedException();
        }

        public void Register<TService>(Func<IContainer, TService> factory) where TService : class
        {
            throw new NotImplementedException();
        }

        public void RegisterAllAsSingleton(Type registrationType, IEnumerable<Type> implementationList)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type service, Type implementation)
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>() where T : class
        {
            return _autoMoqer.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            throw new NotImplementedException();
        }

        void IContainer.Register<TService, TImplementation>()
        {
            throw new NotImplementedException();
        }
    }
}
