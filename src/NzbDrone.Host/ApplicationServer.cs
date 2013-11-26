using System.ServiceProcess;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Host.Owin;

namespace NzbDrone.Host
{
    public interface INzbDroneServiceFactory
    {
        bool IsServiceStopped { get; }
        ServiceBase Build();
        void Start();
    }

    public class NzbDroneServiceFactory : ServiceBase, INzbDroneServiceFactory
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IHostController _hostController;
        private readonly PriorityMonitor _priorityMonitor;
        private readonly IStartupContext _startupContext;
        private readonly IBrowserService _browserService;
        private readonly Logger _logger;

        public NzbDroneServiceFactory(IConfigFileProvider configFileProvider, IHostController hostController,
            IRuntimeInfo runtimeInfo, PriorityMonitor priorityMonitor, IStartupContext startupContext, IBrowserService browserService, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
            _runtimeInfo = runtimeInfo;
            _priorityMonitor = priorityMonitor;
            _startupContext = startupContext;
            _browserService = browserService;
            _logger = logger;
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public void Start()
        {
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
            _logger.Info("Attempting to stop application.");
            _hostController.StopServer();
            _logger.Info("Application has finished stop routine.");
            IsServiceStopped = true;
        }

        public bool IsServiceStopped { get; private set; }

        public ServiceBase Build()
        {
            return this;
        }
    }

}