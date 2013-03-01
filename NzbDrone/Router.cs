using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.SysTray;

namespace NzbDrone
{
    public class Router
    {
        private readonly ApplicationServer _applicationServer;
        private readonly ServiceProvider _serviceProvider;
        private readonly ConsoleProvider _consoleProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly SysTrayProvider _sysTrayProvider;
        private readonly Logger _logger;

        public Router(ApplicationServer applicationServer, ServiceProvider serviceProvider,
                        ConsoleProvider consoleProvider, EnvironmentProvider environmentProvider, SysTrayProvider sysTrayProvider, Logger logger)
        {
            _applicationServer = applicationServer;
            _serviceProvider = serviceProvider;
            _consoleProvider = consoleProvider;
            _environmentProvider = environmentProvider;
            _sysTrayProvider = sysTrayProvider;
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
                        _serviceProvider.Run(_applicationServer);
                        break;
                    }

                case ApplicationModes.Console:
                    {
                        _logger.Trace("Console selected");
                        _applicationServer.Start();
                        if (ConsoleProvider.IsConsoleApplication)
                            _consoleProvider.WaitForClose();

                        else
                        {
                            _sysTrayProvider.Start();
                        }

                        break;
                    }
                case ApplicationModes.InstallService:
                    {
                        _logger.Trace("Install Service selected");
                        if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
                        {
                            _consoleProvider.PrintServiceAlreadyExist();
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
                            _consoleProvider.PrintServiceDoestExist();
                        }
                        else
                        {
                            _serviceProvider.UnInstall(ServiceProvider.NZBDRONE_SERVICE_NAME);
                        }

                        break;
                    }
                default:
                    {
                        _consoleProvider.PrintHelp();
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
