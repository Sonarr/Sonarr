using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.SysTray;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone
{
    [Singleton]
    public class Router
    {
        private readonly INzbDroneServiceFactory _nzbDroneServiceFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsoleService _consoleService;
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly ISystemTrayApp _systemTrayProvider;
        private readonly Logger _logger;

        public Router(INzbDroneServiceFactory nzbDroneServiceFactory, IServiceProvider serviceProvider,
                        IConsoleService consoleService, IEnvironmentProvider environmentProvider, ISystemTrayApp systemTrayProvider, Logger logger)
        {
            _nzbDroneServiceFactory = nzbDroneServiceFactory;
            _serviceProvider = serviceProvider;
            _consoleService = consoleService;
            _environmentProvider = environmentProvider;
            _systemTrayProvider = systemTrayProvider;
            _logger = logger;
        }

        public void Route(IEnumerable<string> args)
        {
            Route(GetApplicationMode(args));
        }

        public void Route(ApplicationModes applicationModes)
        {
            if (!_environmentProvider.IsUserInteractive)
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
                            _consoleService.PrintServiceDoestExist();
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

        public static ApplicationModes GetApplicationMode(IEnumerable<string> args)
        {
            if (args == null) return ApplicationModes.Console;

            var cleanArgs = args.Where(c => c != null && !String.IsNullOrWhiteSpace(c)).ToList();
            if (cleanArgs.Count == 0) return ApplicationModes.Console;
            if (cleanArgs.Count != 1) return ApplicationModes.Help;

            var arg = cleanArgs.First().Trim('/', '\\', '-').ToLower();

            if (arg == "i") return ApplicationModes.InstallService;
            if (arg == "u") return ApplicationModes.UninstallService;

            return ApplicationModes.Help;
        }
    }
}
