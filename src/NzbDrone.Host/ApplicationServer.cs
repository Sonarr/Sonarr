using System;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Host.Owin;

namespace NzbDrone.Host
{
    public interface INzbDroneServiceFactory
    {
        ServiceBase Build();
        void Start();
    }

    public class NzbDroneServiceFactory : ServiceBase, INzbDroneServiceFactory, IHandle<ApplicationShutdownRequested>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IHostController _hostController;
        private readonly PriorityMonitor _priorityMonitor;
        private readonly IStartupContext _startupContext;
        private readonly IBrowserService _browserService;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public NzbDroneServiceFactory(IConfigFileProvider configFileProvider, 
                                      IHostController hostController,
                                      IRuntimeInfo runtimeInfo, 
                                      PriorityMonitor priorityMonitor, 
                                      IStartupContext startupContext, 
                                      IBrowserService browserService, 
                                      IProcessProvider processProvider, 
                                      Logger logger)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
            _runtimeInfo = runtimeInfo;
            _priorityMonitor = priorityMonitor;
            _startupContext = startupContext;
            _browserService = browserService;
            _processProvider = processProvider;
            _logger = logger;
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public void Start()
        {
            if (OsInfo.IsLinux)
            {
                Console.CancelKeyPress += (sender, eventArgs) => _processProvider.Kill(_processProvider.GetCurrentProcess().Id);
            }

            _runtimeInfo.IsRunning = true;
            _hostController.StartServer();

            if (!_startupContext.Flags.Contains(StartupContext.NO_BROWSER)
                && _configFileProvider.LaunchBrowser)
            {
                _browserService.LaunchWebUI();
            }

            _priorityMonitor.Start();
        }

        protected override void OnStop()
        {
            Shutdown();
        }

        public ServiceBase Build()
        {
            return this;
        }

        private void Shutdown()
        {
            _logger.Info("Attempting to stop application.");
            _hostController.StopServer();
            _logger.Info("Application has finished stop routine.");
            _runtimeInfo.IsRunning = false;
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            if (OsInfo.IsLinux)
            {
                _processProvider.Kill(_processProvider.GetCurrentProcess().Id);
            }

            if (!_runtimeInfo.IsWindowsService && !message.Restarting)
            {
                Shutdown();
            }
        }
    }
}