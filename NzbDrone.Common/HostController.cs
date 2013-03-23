using System;
using System.Linq;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;

namespace NzbDrone.Common
{
    public interface IHostController
    {
        bool ServerStarted { get; }
        string AppUrl { get; }
        void StartServer();
        void RestartServer();
        void StopServer();
    }

    public class HostController : IHostController
    {
        private readonly ConfigFileProvider _configFileProvider;
        private readonly INancyBootstrapper _bootstrapper;
        private readonly Logger _logger;
        private NancyHost _host;


        public bool ServerStarted { get; private set; }

        public HostController(ConfigFileProvider configFileProvider, INancyBootstrapper bootstrapper, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _bootstrapper = bootstrapper;
            _logger = logger;
        }

        public void StartServer()
        {
            //Todo: We need this to be able run when the user isn't an admin
            //Todo: And when the URL hasn't been registered in URL ACL: netsh http add urlacl url=http://+:8989/ user=everyone
            _host = new NancyHost(new Uri(AppUrl), _bootstrapper);
            _host.Start();
        }

        public string AppUrl
        {
            get { return string.Format("http://localhost:{0}", _configFileProvider.Port); }
        }


        public void RestartServer()
        {

            _logger.Warn("Attempting to restart server.");

            if (_host != null)
            {
                StopServer();
            }

            StartServer();
        }

        public void StopServer()
        {
            if (_host == null) return;

            _logger.Info("Attempting to stop Nancy host");
            _host.Stop();
            _host = null;
            _logger.Info("Host has stopped");
        }
    }
}