using System;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common;
using NzbDrone.Owin;

namespace NzbDrone
{
    public interface INzbDroneServiceFactory
    {
        ServiceBase Build();
        void Start();
    }

    public class NzbDroneServiceFactory : ServiceBase, INzbDroneServiceFactory
    {
        private readonly ConfigFileProvider _configFileProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly IHostController _hostController;
        private readonly ProcessProvider _processProvider;
        private readonly PriorityMonitor _priorityMonitor;
        private readonly SecurityProvider _securityProvider;
        private readonly Logger _logger;

        public NzbDroneServiceFactory(ConfigFileProvider configFileProvider, IHostController hostController,
                          EnvironmentProvider environmentProvider,
                           ProcessProvider processProvider, PriorityMonitor priorityMonitor,
                           SecurityProvider securityProvider, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
            _environmentProvider = environmentProvider;
            _processProvider = processProvider;
            _priorityMonitor = priorityMonitor;
            _securityProvider = securityProvider;
            _logger = logger;
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public void Start()
        {
            _securityProvider.MakeAccessible();

            _hostController.StartServer();

            if (_environmentProvider.IsUserInteractive && _configFileProvider.LaunchBrowser)
            {
                try
                {
                    _logger.Info("Starting default browser. {0}", _hostController.AppUrl);
                    _processProvider.Start(_hostController.AppUrl);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Failed to open URL in default browser.", e);
                }
            }

            _priorityMonitor.Start();
        }

        protected override void OnStop()
        {
            _logger.Info("Attempting to stop application.");
            _hostController.StopServer();
            _logger.Info("Application has finished stop routine.");
        }

        public ServiceBase Build()
        {
            return this;
        }
    }

}