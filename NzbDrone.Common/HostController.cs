using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Owin;

namespace NzbDrone.Common
{
    public interface IHostController
    {
        string AppUrl { get; }
        void StartServer();
        void RestartServer();
        void StopServer();
    }


    public class OwinHostController : IHostController
    {
        private readonly ConfigFileProvider _configFileProvider;
        private readonly INancyBootstrapper _bootstrapper;
        private readonly Logger _logger;
        private IDisposable _host;

        public OwinHostController(ConfigFileProvider configFileProvider, INancyBootstrapper bootstrapper, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _bootstrapper = bootstrapper;
            _logger = logger;
        }

        public void StartServer()
        {
            _host = WebApplication.Start(AppUrl, builder => RunNancy(builder, _bootstrapper));
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