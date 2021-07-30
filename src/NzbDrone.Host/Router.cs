using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Host.AccessControl;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone.Host
{
    public class Router
    {
        private readonly INzbDroneConsoleFactory _nzbDroneConsoleFactory;
        private readonly INzbDroneServiceFactory _nzbDroneServiceFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsoleService _consoleService;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IProcessProvider _processProvider;
        private readonly IRemoteAccessAdapter _remoteAccessAdapter;
        private readonly Logger _logger;

        public Router(INzbDroneConsoleFactory nzbDroneConsoleFactory,
                      INzbDroneServiceFactory nzbDroneServiceFactory,
                      IServiceProvider serviceProvider,
                      IConsoleService consoleService,
                      IRuntimeInfo runtimeInfo,
                      IProcessProvider processProvider,
                      IRemoteAccessAdapter remoteAccessAdapter,
                      Logger logger)
        {
            _nzbDroneConsoleFactory = nzbDroneConsoleFactory;
            _nzbDroneServiceFactory = nzbDroneServiceFactory;
            _serviceProvider = serviceProvider;
            _consoleService = consoleService;
            _runtimeInfo = runtimeInfo;
            _processProvider = processProvider;
            _remoteAccessAdapter = remoteAccessAdapter;
            _logger = logger;
        }

        public void Route(ApplicationModes applicationModes)
        {
            _logger.Info("Application mode: {0}", applicationModes);

            switch (applicationModes)
            {
                case ApplicationModes.Service:
                    {
                        _logger.Debug("Service selected");

                        _serviceProvider.Run(_nzbDroneServiceFactory.Build());

                        break;
                    }

                case ApplicationModes.Interactive:
                    {
                        _logger.Debug(_runtimeInfo.IsWindowsTray ? "Tray selected" : "Console selected");
                        _nzbDroneConsoleFactory.Start();
                        break;
                    }
                case ApplicationModes.InstallService:
                    {
                        _logger.Debug("Install Service selected");
                        if (_serviceProvider.ServiceExist(ServiceProvider.SERVICE_NAME))
                        {
                            _consoleService.PrintServiceAlreadyExist();
                        }
                        else
                        {
                            _remoteAccessAdapter.MakeAccessible(true);
                            _serviceProvider.Install(ServiceProvider.SERVICE_NAME);
                            _serviceProvider.SetPermissions(ServiceProvider.SERVICE_NAME);

                            // Start the service and exit.
                            // Ensures that there isn't an instance of Sonarr already running that the service account cannot stop.
                            _processProvider.SpawnNewProcess("sc.exe", $"start {ServiceProvider.SERVICE_NAME}", null, true);
                        }
                        break;
                    }
                case ApplicationModes.UninstallService:
                    {
                        _logger.Debug("Uninstall Service selected");
                        if (!_serviceProvider.ServiceExist(ServiceProvider.SERVICE_NAME))
                        {
                            _consoleService.PrintServiceDoesNotExist();
                        }
                        else
                        {
                            _serviceProvider.Uninstall(ServiceProvider.SERVICE_NAME);
                        }

                        break;
                    }
                case ApplicationModes.RegisterUrl:
                    {
                        _logger.Debug("Regiser URL selected");
                        _remoteAccessAdapter.MakeAccessible(false);

                        break;
                    }
                default:
                    {
                        _consoleService.PrintHelp();
                        break;
                    }
            }
        }
    }
}
