using System.ServiceProcess;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Host
{
    public class Router
    {
        private readonly INzbDroneServiceFactory _nzbDroneServiceFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IStartupArguments _startupArguments;
        private readonly IConsoleService _consoleService;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public Router(INzbDroneServiceFactory nzbDroneServiceFactory, IServiceProvider serviceProvider, IStartupArguments startupArguments,
                        IConsoleService consoleService, IRuntimeInfo runtimeInfo, Logger logger)
        {
            _nzbDroneServiceFactory = nzbDroneServiceFactory;
            _serviceProvider = serviceProvider;
            _startupArguments = startupArguments;
            _consoleService = consoleService;
            _runtimeInfo = runtimeInfo;
            _logger = logger;
        }

        public void Route()
        {
            var appMode = GetApplicationMode();
            Route(appMode);
        }

        public void Route(ApplicationModes applicationModes)
        {
            _logger.Info("Application mode: {0}", applicationModes);

            switch (applicationModes)
            {
                case ApplicationModes.Service:
                    {
                        _logger.Trace("Service selected");
                        _serviceProvider.Run(_nzbDroneServiceFactory.Build());
                        break;
                    }

                case ApplicationModes.Interactive:
                    {
                        _logger.Trace("Console selected");
                        _nzbDroneServiceFactory.Start();
                        break;
                    }
                case ApplicationModes.InstallService:
                    {
                        _logger.Trace("Install Service selected");
                        if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
                        {
                            _consoleService.PrintServiceAlreadyExist();
                        }
                        else
                        {
                            _serviceProvider.Install(ServiceProvider.NZBDRONE_SERVICE_NAME);
                            _serviceProvider.Start(ServiceProvider.NZBDRONE_SERVICE_NAME);
                        }
                        break;
                    }
                case ApplicationModes.UninstallService:
                    {
                        _logger.Trace("Uninstall Service selected");
                        if (!_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
                        {
                            _consoleService.PrintServiceDoesNotExist();
                        }
                        else
                        {
                            _serviceProvider.UnInstall(ServiceProvider.NZBDRONE_SERVICE_NAME);
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

        private ApplicationModes GetApplicationMode()
        {
            if (!_runtimeInfo.IsUserInteractive &&
               OsInfo.IsWindows &&
               _serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME) &&
               _serviceProvider.GetStatus(ServiceProvider.NZBDRONE_SERVICE_NAME) == ServiceControllerStatus.StartPending)
            {
                return ApplicationModes.Service;
            }

            if (_startupArguments.Flags.Contains(StartupArguments.HELP))
            {
                return ApplicationModes.Help;
            }

            if (!OsInfo.IsLinux && _startupArguments.Flags.Contains(StartupArguments.INSTALL_SERVICE))
            {
                return ApplicationModes.InstallService;
            }

            if (!OsInfo.IsLinux && _startupArguments.Flags.Contains(StartupArguments.UNINSTALL_SERVICE))
            {
                return ApplicationModes.UninstallService;
            }

            return ApplicationModes.Interactive;
        }
    }
}
