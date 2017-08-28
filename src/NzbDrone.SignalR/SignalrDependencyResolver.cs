using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Common.Composition;

namespace NzbDrone.SignalR
{
    public class SignalRDependencyResolver : DefaultDependencyResolver
    {
        private readonly IContainer _container;

        public static void Register(IContainer container)
        {
            GlobalHost.DependencyResolver = new SignalRDependencyResolver(container);
        }

        private SignalRDependencyResolver(IContainer container)
        {
            _container = container;
            var performanceCounterManager = new SonarrPerformanceCounterManager();
            Register(typeof(IPerformanceCounterManager), () => performanceCounterManager);
        }

        public override object GetService(Type serviceType)
        {
            // Microsoft.AspNet.SignalR.Infrastructure.AckSubscriber is not registered in our internal contaiiner,
            // but it still gets treated like it is (possibly due to being a concrete type).

            var fullName = serviceType.FullName;

            if (fullName == "Microsoft.AspNet.SignalR.Infrastructure.AckSubscriber" ||
                fullName == "Newtonsoft.Json.JsonSerializer")
            {
                return base.GetService(serviceType);
            }

            if (_container.IsTypeRegistered(serviceType))
            {
                return _container.Resolve(serviceType);
            }

            return base.GetService(serviceType);
        }
    }
}