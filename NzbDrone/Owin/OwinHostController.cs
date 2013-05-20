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
using System.Linq;

namespace NzbDrone.Owin
{
    public class OwinHostController : IHostController
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEnumerable<IOwinMiddleWare> _owinMiddleWares;
        private readonly Logger _logger;
        private IDisposable _host;

        public OwinHostController(IConfigFileProvider configFileProvider, IEnumerable<IOwinMiddleWare> owinMiddleWares, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _owinMiddleWares = owinMiddleWares;
            _logger = logger;
        }

        public void StartServer()
        {
            var options = new StartOptions
                {
                    App = GetType().AssemblyQualifiedName,
                    Port = _configFileProvider.Port
                };

            _host = WebApplication.Start(options, BuildApp);
        }

        private void BuildApp(IAppBuilder appBuilder)
        {
            foreach (var middleWare in _owinMiddleWares.OrderBy(c => c.Order))
            {
                _logger.Debug("Attaching {0} to host", middleWare.GetType().Name);
                middleWare.Attach(appBuilder);
            }
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