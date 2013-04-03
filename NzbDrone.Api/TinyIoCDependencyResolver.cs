using System;
using System.Collections.Generic;
using System.Linq;
using SignalR;
using TinyIoC;

namespace NzbDrone.Api
{
    public class TinyIoCDependencyResolver : DefaultDependencyResolver
    {
        private readonly TinyIoCContainer _container;

        public TinyIoCDependencyResolver(TinyIoCContainer container)
        {
            _container = container;
        }

        public override object GetService(Type serviceType)
        {
            if (_container.CanResolve(serviceType))
            {
                return _container.Resolve(serviceType);
            }

            return base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            IEnumerable<object> services = new object[] { };
            
            if (_container.CanResolve(serviceType))
            {
                services = _container.ResolveAll(serviceType);
            }

            return services.Concat(base.GetServices(serviceType));
        }
    }
}