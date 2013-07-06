using System;
using System.Collections.Generic;
using Microsoft.Owin.Hosting;
using NLog;
using NzbDrone.Core.Configuration;
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
            var url = "http://*:" + _configFileProvider.Port;

            var options = new StartOptions(url)
                {
                    ServerFactory = "Microsoft.Owin.Host.HttpListener"
                };

            _logger.Info("starting server on {0}", url);

            _host = WebApp.Start(options, BuildApp);
        }

        private void BuildApp(IAppBuilder appBuilder)
        {
            appBuilder.Properties["host.AppName"] = "NzbDrone";

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