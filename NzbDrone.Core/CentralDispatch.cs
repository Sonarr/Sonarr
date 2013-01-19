using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers.Core;
using SignalR;

namespace NzbDrone.Core
{
    public class CentralDispatch
    {
        private readonly Logger _logger;
        private readonly EnvironmentProvider _environmentProvider;

        public ContainerBuilder ContainerBuilder { get; private set; }

        public CentralDispatch()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _environmentProvider = new EnvironmentProvider();

            _logger.Debug("Initializing ContainerBuilder:");
            ContainerBuilder = new ContainerBuilder();

        }

        private void RegisterReporting(IContainer container)
        {
            EnvironmentProvider.UGuid = container.Resolve<ConfigProvider>().UGuid;
            ReportingService.RestProvider = container.Resolve<RestProvider>();
            ReportingService.SetupExceptronDriver();
        }


        public void DedicateToHost()
        {
            try
            {
                var pid = _environmentProvider.NzbDroneProcessIdFromEnviroment;

                _logger.Debug("Attaching to parent process ({0}) for automatic termination.", pid);

                var hostProcess = Process.GetProcessById(Convert.ToInt32(pid));

                hostProcess.EnableRaisingEvents = true;
                hostProcess.Exited += (delegate
                                           {
                                               _logger.Info("Host has been terminated. Shutting down web server.");
                                               ShutDown();
                                           });

                _logger.Debug("Successfully Attached to host. Process [{0}]", hostProcess.ProcessName);
            }
            catch (Exception e)
            {
                _logger.FatalException("An error has occurred while dedicating to host.", e);
            }
        }

        public IContainer BuildContainer()
        {
            _logger.Debug("Initializing Components");

            ContainerBuilder.RegisterCoreServices();

            var container = ContainerBuilder.Build();

            container.Resolve<DatabaseTarget>().Register();
            LogConfiguration.Reload();

            RegisterReporting(container);

            container.Resolve<WebTimer>().StartTimer(30);

            //SignalR
            GlobalHost.DependencyResolver = new AutofacSignalrDependencyResolver(container.BeginLifetimeScope("SignalR"));

            return container;
        }

        private void ShutDown()
        {
            _logger.Info("Shutting down application...");
            WebTimer.Stop();
            Process.GetCurrentProcess().Kill();
        }
    }
}
