using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Owin;
using NzbDrone.Common;
using NzbDrone.Owin.MiddleWare;
using Owin;

namespace NzbDrone.Owin
{
    public class OwinHostController : IHostController
    {
        private readonly ConfigFileProvider _configFileProvider;
        private readonly IEnumerable<IOwinMiddleWare> _owinMiddleWares;
        private readonly Logger _logger;
        private IDisposable _host;

        public OwinHostController(ConfigFileProvider configFileProvider, IEnumerable<IOwinMiddleWare> owinMiddleWares, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _owinMiddleWares = owinMiddleWares;
            _logger = logger;
        }

        public void StartServer()
        {
            _host = WebApplication.Start(AppUrl, BuildApp);
        }

        private void BuildApp(IAppBuilder appBuilder)
        {
            foreach (var middleWare in _owinMiddleWares)
            {
                middleWare.Attach(appBuilder);
            }
        }

        private static IAppBuilder RunNancy(IAppBuilder builder, INancyBootstrapper bootstrapper)
        {
            var nancyOwinHost = new NancyOwinHost(null, bootstrapper);
            return builder.Use((Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>)(next => (Func<IDictionary<string, object>, Task>)nancyOwinHost.Invoke), new object[0]);
        }

        public string AppUrl
        {
            get { return string.Format("http://localhost:{0}", _configFileProvider.Port); }
        }

        public void RestartServer()
        {
            _logger.Warn("Attempting to restart server.");

            StopServer();
            StartServer();
        }

        public void StopServer()
        {
            if (_host == null) return;

            _logger.Info("Attempting to stop Nancy host");
            _host.Dispose();
            _host = null;
            _logger.Info("Host has stopped");
        }

    }
}