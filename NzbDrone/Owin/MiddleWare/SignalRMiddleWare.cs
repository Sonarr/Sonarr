using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Owin;

namespace NzbDrone.Owin.MiddleWare
{
    public class SignalRMiddleWare : IOwinMiddleWare
    {
        private readonly INancyBootstrapper _nancyBootstrapper;

        public SignalRMiddleWare(INancyBootstrapper nancyBootstrapper)
        {
            _nancyBootstrapper = nancyBootstrapper;
        }

        public void Attach(IAppBuilder appBuilder)
        {
            return;
            var nancyOwinHost = new NancyOwinHost(null, _nancyBootstrapper);
            appBuilder.Use((Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>)(next => (Func<IDictionary<string, object>, Task>)nancyOwinHost.Invoke), new object[0]);
        }
    }
}