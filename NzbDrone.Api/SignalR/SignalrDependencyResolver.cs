using System;
using Microsoft.AspNet.SignalR;
using NzbDrone.Common.Composition;

namespace NzbDrone.Api.SignalR
{
    public class SignalrDependencyResolver : DefaultDependencyResolver
    {
        private readonly IContainer _container;

        public static void Register(IContainer container)
        {
            GlobalHost.DependencyResolver = new SignalrDependencyResolver(container);
        }

        private SignalrDependencyResolver(IContainer container)
        {
            _container = container;
        }

        public override object GetService(Type serviceType)
        {
            if (_container.IsTypeRegistered(serviceType))
            {
                return _container.Resolve(serviceType);
            }

            return base.GetService(serviceType);
        }
    }
}
