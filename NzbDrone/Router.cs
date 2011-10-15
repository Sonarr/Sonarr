using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Model;
using NzbDrone.Providers;

namespace NzbDrone
{
    public class Router
    {
        private static readonly Logger Logger = LogManager.GetLogger("Host.Router");

        private readonly ApplicationServer _applicationServer;
        private readonly ServiceProvider _serviceProvider;
        private readonly ConsoleProvider _consoleProvider;
        private readonly EnviromentProvider _enviromentProvider;

        public Router(ApplicationServer applicationServer, ServiceProvider serviceProvider, ConsoleProvider consoleProvider, EnviromentProvider enviromentProvider)
        {
            _applicationServer = applicationServer;
            _serviceProvider = serviceProvider;
            _consoleProvider = consoleProvider;
            _enviromentProvider = enviromentProvider;
        }

        public void Route(IEnumerable<string> args)
        {
            Route(GetApplicationMode(args));
        }

        public void Route(ApplicationMode applicationMode)
        {
            Logger.Info("Application mode: {0}", applicationMode);

            if (!_enviromentProvider.IsUserInteractive)
            {
                _serviceProvider.Run(_applicationServer);
            }
            else
            {
                switch (applicationMode)
                {

                    case ApplicationMode.Console:
                        {
                            _applicationServer.Start();
                            _consoleProvider.WaitForClose();
                            break;
                        }
                    case ApplicationMode.InstallService:
                        {
                            if (_serviceProvider.ServiceExist(ServiceProvider.NzbDroneServiceName))
                            {
                                _consoleProvider.PrintServiceAlreadyExist();
                            }
                            else
                            {
                                _serviceProvider.Install();
                            }
                            break;
                        }
                    case ApplicationMode.UninstallService:
                        {
                            if (!_serviceProvider.ServiceExist(ServiceProvider.NzbDroneServiceName))
                            {
                                _consoleProvider.PrintServiceDoestExist();
                            }
                            else
                            {
                                _serviceProvider.UnInstall();
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
        }

        public static ApplicationMode GetApplicationMode(IEnumerable<string> args)
        {
            if (args == null) return ApplicationMode.Console;

            var cleanArgs = args.Where(c => c != null && !String.IsNullOrWhiteSpace(c)).ToList();
            if (cleanArgs.Count == 0) return ApplicationMode.Console;
            if (cleanArgs.Count != 1) return ApplicationMode.Help;

            var arg = cleanArgs.First().Trim('/', '\\', '-').ToLower();

            if (arg == "i") return ApplicationMode.InstallService;
            if (arg == "u") return ApplicationMode.UninstallService;

            return ApplicationMode.Help;
        }
    }
}
