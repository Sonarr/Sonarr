using System;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Host
{
    public interface INzbDroneServiceFactory
    {
        ServiceBase Build();
    }

    public interface INzbDroneConsoleFactory
    {
        void Start();
        void Shutdown();
    }

    public class NzbDroneServiceFactory : ServiceBase, INzbDroneServiceFactory
    {
        private readonly INzbDroneConsoleFactory _consoleFactory;

        public NzbDroneServiceFactory(INzbDroneConsoleFactory consoleFactory)
        {
            _consoleFactory = consoleFactory;
        }

        protected override void OnStart(string[] args)
        {
            _consoleFactory.Start();
        }

        protected override void OnStop()
        {
            _consoleFactory.Shutdown();
        }

        public ServiceBase Build()
        {
            return this;
        }
    }

    public class DummyNzbDroneServiceFactory : INzbDroneServiceFactory
    {
        public ServiceBase Build()
        {
            return null;
        }
    }

    public class NzbDroneConsoleFactory : INzbDroneConsoleFactory, IHandle<ApplicationShutdownRequested>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IHostController _hostController;
        private readonly IStartupContext _startupContext;
        private readonly IBrowserService _browserService;
        private readonly IContainer _container;
        private readonly Logger _logger;

        // private CancelHandler _cancelHandler;
        public NzbDroneConsoleFactory(IConfigFileProvider configFileProvider,
                                      IHostController hostController,
                                      IRuntimeInfo runtimeInfo,
                                      IStartupContext startupContext,
                                      IBrowserService browserService,
                                      IContainer container,
                                      Logger logger)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
            _runtimeInfo = runtimeInfo;
            _startupContext = startupContext;
            _browserService = browserService;
            _container = container;
            _logger = logger;
        }

        public void Start()
        {
            if (OsInfo.IsNotWindows)
            {
                //Console.CancelKeyPress += (sender, eventArgs) => eventArgs.Cancel = true;
                //_cancelHandler = new CancelHandler();
            }

            _runtimeInfo.IsExiting = false;
            DbFactory.RegisterDatabase(_container);

            _container.Resolve<IEventAggregator>().PublishEvent(new ApplicationStartingEvent());

            if (_runtimeInfo.IsExiting)
            {
                return;
            }

            _hostController.StartServer();

            if (!_startupContext.Flags.Contains(StartupContext.NO_BROWSER)
                && _configFileProvider.LaunchBrowser)
            {
                _browserService.LaunchWebUI();
            }

            _container.Resolve<IEventAggregator>().PublishEvent(new ApplicationStartedEvent());
        }

        public void Shutdown()
        {
            _logger.Info("Attempting to stop application.");
            _hostController.StopServer();
            _logger.Info("Application has finished stop routine.");
            _runtimeInfo.IsExiting = true;
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            if (!_runtimeInfo.IsWindowsService)
            {
                if (message.Restarting)
                {
                    _runtimeInfo.RestartPending = true;
                }

                LogManager.Configuration = null;
                Shutdown();
            }
        }
    }
}
