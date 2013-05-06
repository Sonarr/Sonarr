using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Json;
using TinyIoC;

namespace NzbDrone.Api.SignalR
{
    public class SignalrDependencyResolver : DefaultDependencyResolver
    {
        private readonly TinyIoCContainer _container;

        public static void Register(TinyIoCContainer container)
        {
            GlobalHost.DependencyResolver = new SignalrDependencyResolver(container);
            
            container.Register<IJsonSerializer, Serializer>().AsSingleton();
        }

        private SignalrDependencyResolver(TinyIoCContainer container)
        {
            _container = container;
        }

        public override object GetService(Type serviceType)
        {
            return _container.CanResolve(serviceType) ? _container.Resolve(serviceType) : base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var objects = _container.CanResolve(serviceType) ? _container.ResolveAll(serviceType) : new object[] { };
            return objects.Concat(base.GetServices(serviceType));
        }
    }
}
