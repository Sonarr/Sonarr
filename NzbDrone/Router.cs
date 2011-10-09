using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Providers;

namespace NzbDrone
{
    class Router
    {
        private readonly Application _application;
        private readonly ServiceProvider _serviceProvider;
        private readonly ConsoleProvider _consoleProvider;
        private readonly ApplicationMode _applicationMode;


        public Router(Application application, ServiceProvider serviceProvider, ConsoleProvider consoleProvider, ApplicationMode applicationMode)
        {
            _application = application;
            _serviceProvider = serviceProvider;
            _consoleProvider = consoleProvider;
            _applicationMode = applicationMode;
        }

        public void Route()
        {
            switch (_applicationMode)
            {
                case ApplicationMode.Console:
                    {
                        _application.Start();
                        _consoleProvider.WaitForClose();
                        break;
                    }
                case ApplicationMode.InstallService:
                    {
                        _serviceProvider.Install();
                        break;
                    }
                case ApplicationMode.UninstallService:
                    {
                        _serviceProvider.UnInstall();
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
}
