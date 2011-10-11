using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Model;
using NzbDrone.Providers;

namespace NzbDrone
{
    class Router
    {
        private readonly ApplicationServer _applicationServer;
        private readonly ServiceProvider _serviceProvider;
        private readonly ConsoleProvider _consoleProvider;

        public Router(ApplicationServer applicationServer, ServiceProvider serviceProvider, ConsoleProvider consoleProvider)
        {
            _applicationServer = applicationServer;
            _serviceProvider = serviceProvider;
            _consoleProvider = consoleProvider;          
        }

        public void Route()
        {
            switch (CentralDispatch.ApplicationMode)
            {
                case ApplicationMode.Console:
                    {
                        _applicationServer.Start();
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
