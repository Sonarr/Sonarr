using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Host
{
    public class Router
    {
        private readonly INzbDroneServiceFactory _nzbDroneServiceFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsoleService _consoleService;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public Router(INzbDroneServiceFactory nzbDroneServiceFactory,
                      IServiceProvider serviceProvider,
                      IConsoleService consoleService,
                      IRuntimeInfo runtimeInfo,
                      Logger logger)
        {
            _nzbDroneServiceFactory = nzbDroneServiceFactory;
            _serviceProvider = serviceProvider;
            _consoleService = consoleService;
            _runtimeInfo = runtimeInfo;
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

                        _nzbDroneServiceFactory.Start();

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
                            _serviceProvider.Install(ServiceProvider.SERVICE_NAME);
                            _serviceProvider.Start(ServiceProvider.SERVICE_NAME);
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
                            _serviceProvider.UnInstall(ServiceProvider.SERVICE_NAME);
                        }

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
