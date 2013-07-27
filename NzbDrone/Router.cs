using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.SysTray;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone
{
    public class Router
    {
        private readonly INzbDroneServiceFactory _nzbDroneServiceFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly StartupArguments _startupArguments;
        private readonly IConsoleService _consoleService;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly ISystemTrayApp _systemTrayProvider;
        private readonly Logger _logger;

        public Router(INzbDroneServiceFactory nzbDroneServiceFactory, IServiceProvider serviceProvider, StartupArguments startupArguments,
                        IConsoleService consoleService, IRuntimeInfo runtimeInfo, ISystemTrayApp systemTrayProvider, Logger logger)
        {
            _nzbDroneServiceFactory = nzbDroneServiceFactory;
            _serviceProvider = serviceProvider;
            _startupArguments = startupArguments;
            _consoleService = consoleService;
            _runtimeInfo = runtimeInfo;
            _systemTrayProvider = systemTrayProvider;
            _logger = logger;
        }

        public void Route()
        {
            var appMode = GetApplicationMode();
            Route(appMode);
        }

        public void Route(ApplicationModes applicationModes)
        {
            if (!_runtimeInfo.IsUserInteractive && !OsInfo.IsLinux)
            {
                applicationModes = ApplicationModes.Service;
            }

            _logger.Info("Application mode: {0}", applicationModes);

            switch (applicationModes)
            {
                case ApplicationModes.Service:
                    {
                        _logger.Trace("Service selected");
                        _serviceProvider.Run(_nzbDroneServiceFactory.Build());
                        break;
                    }

                case ApplicationModes.Console:
                    {
                        _logger.Trace("Console selected");
                        _nzbDroneServiceFactory.Start();
                        if (_consoleService.IsConsoleApplication)
                        {
                            _consoleService.WaitForClose();
                        }
                        else
                        {
                            _systemTrayProvider.Start();
                        }

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

            return ApplicationModes.Console;
        }
    }
}
