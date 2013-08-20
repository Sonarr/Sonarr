using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Owin;

namespace NzbDrone.Host.Owin.MiddleWare
{
    public class NancyMiddleWare : IOwinMiddleWare
    {
        private readonly INancyBootstrapper _nancyBootstrapper;

        public NancyMiddleWare(INancyBootstrapper nancyBootstrapper)
        {
            _nancyBootstrapper = nancyBootstrapper;
        }

        public int Order { get { return 1; } }

        public void Attach(IAppBuilder appBuilder)
        {
            var nancyOwinHost = new NancyOwinHost(null, _nancyBootstrapper, new HostConfiguration());
            appBuilder.Use((Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>)(next => (Func<IDictionary<string, object>, Task>)nancyOwinHost.Invoke), new object[0]);
        }
    }
}