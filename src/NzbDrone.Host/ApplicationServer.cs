using System;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Host.Owin;

namespace NzbDrone.Host
{
    public interface INzbDroneServiceFactory
    {
        ServiceBase Build();
        void Start();
    }

    public class NzbDroneServiceFactory : ServiceBase, INzbDroneServiceFactory
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IHostController _hostController;
        private readonly IProcessProvider _processProvider;
        private readonly PriorityMonitor _priorityMonitor;
        private readonly IStartupArguments _startupArguments;
        private readonly Logger _logger;

        public NzbDroneServiceFactory(IConfigFileProvider configFileProvider, IHostController hostController, IRuntimeInfo runtimeInfo,
                           IProcessProvider processProvider, PriorityMonitor priorityMonitor, IStartupArguments startupArguments, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
            _runtimeInfo = runtimeInfo;
            _processProvider = processProvider;
            _priorityMonitor = priorityMonitor;
            _startupArguments = startupArguments;
            _logger = logger;
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public void Start()
        {
            _hostController.StartServer();

            if (!_startupArguments.Flags.Contains(StartupArguments.NO_BROWSER) &&
                _runtimeInfo.IsUserInteractive &&
                _configFileProvider.LaunchBrowser)
            {
                try
                {
                    _logger.Info("Starting default browser. {0}", _hostController.AppUrl);
                    _processProvider.OpenDefaultBrowser(_hostController.AppUrl);
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