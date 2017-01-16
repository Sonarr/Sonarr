using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Common.Composition;

namespace NzbDrone.SignalR
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
            var performanceCounterManager = new SonarrPerformanceCounterManager();
            Register(typeof(IPerformanceCounterManager), () => performanceCounterManager);
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