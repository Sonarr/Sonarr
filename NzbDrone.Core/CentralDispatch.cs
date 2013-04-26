using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Core;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Metadata;
using NzbDrone.Core.Repository;
using PetaPoco;
using SignalR;
using Connection = NzbDrone.Core.Datastore.Connection;

namespace NzbDrone.Core
{
    public class CentralDispatch
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly EnvironmentProvider _environmentProvider;

        public ContainerBuilder ContainerBuilder { get; private set; }

        public CentralDispatch()
        {
            _environmentProvider = new EnvironmentProvider();

            logger.Debug("Initializing ContainerBuilder:");
            ContainerBuilder = new ContainerBuilder();

            ContainerBuilder.RegisterAssemblyTypes(typeof(DiskProvider).Assembly).SingleInstance();
            ContainerBuilder.RegisterAssemblyTypes(typeof(CentralDispatch).Assembly).SingleInstance();
            ContainerBuilder.RegisterType<EnvironmentProvider>();

            InitDatabase();
            RegisterExternalNotifications();
            RegisterMetadataProviders();
            RegisterIndexers();
            RegisterJobs();         
        }

        private void InitDatabase()
        {
            logger.Info("Registering Database...");

            var appDataPath = _environmentProvider.GetAppDataPath();
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            ContainerBuilder.Register(c => c.Resolve<Connection>().GetMainPetaPocoDb())
                            .As<IDatabase>();

            ContainerBuilder.Register(c => c.Resolve<Connection>().GetLogPetaPocoDb(false))
                            .SingleInstance()
                            .Named<IDatabase>("DatabaseTarget");

            ContainerBuilder.Register(c => c.Resolve<Connection>().GetLogPetaPocoDb())
                            .Named<IDatabase>("LogProvider");

            ContainerBuilder.RegisterType<DatabaseTarget>().WithParameter(ResolvedParameter.ForNamed<IDatabase>("DatabaseTarget"));
            ContainerBuilder.RegisterType<LogProvider>().WithParameter(ResolvedParameter.ForNamed<IDatabase>("LogProvider"));
        }

        private void RegisterIndexers()
        {
            logger.Debug("Registering Indexers...");

            ContainerBuilder.RegisterAssemblyTypes(typeof(CentralDispatch).Assembly)
                   .Where(t => t.BaseType == typeof(IndexerBase))
                   .As<IndexerBase>();
        }

        private void RegisterJobs()
        {
            logger.Debug("Registering Background Jobs...");

            ContainerBuilder.RegisterType<JobProvider>().SingleInstance();

            ContainerBuilder.RegisterAssemblyTypes(typeof(CentralDispatch).Assembly)
                   .Where(t => t.GetInterfaces().Contains(typeof(IJob)))
                   .As<IJob>()
                   .SingleInstance();
        }

        private void RegisterExternalNotifications()
        {
            logger.Debug("Registering External Notifications...");

            ContainerBuilder.RegisterAssemblyTypes(typeof(CentralDispatch).Assembly)
                   .Where(t => t.BaseType == typeof(ExternalNotificationBase))
                   .As<ExternalNotificationBase>();     
        }

        private void RegisterMetadataProviders()
        {
            logger.Debug("Registering Metadata Providers...");

            ContainerBuilder.RegisterAssemblyTypes(typeof(CentralDispatch).Assembly)
                   .Where(t => t.IsSubclassOf(typeof(MetadataBase)))
                   .As<MetadataBase>();
        }

        private void RegisterReporting(IContainer container)
        {
            EnvironmentProvider.UGuid = container.Resolve<ConfigProvider>().UGuid;
            ReportingService.RestProvider = container.Resolve<RestProvider>();
        }

        private void RegisterQuality(IContainer container)
        {
            logger.Debug("Initializing Quality...");
            container.Resolve<QualityProvider>().SetupDefaultProfiles();
            container.Resolve<QualityTypeProvider>().SetupDefault();
        }

        public void DedicateToHost()
        {
            try
            {
                var pid = _environmentProvider.NzbDroneProcessIdFromEnviroment;

                logger.Debug("Attaching to parent process ({0}) for automatic termination.", pid);

                var hostProcess = Process.GetProcessById(Convert.ToInt32(pid));

                hostProcess.EnableRaisingEvents = true;
                hostProcess.Exited += (delegate
                                           {
                                               logger.Info("Host has been terminated. Shutting down web server.");
                                               ShutDown();
                                           });

                logger.Debug("Successfully Attached to host. Process [{0}]", hostProcess.ProcessName);
            }
            catch (Exception e)
            {
                logger.FatalException("An error has occurred while dedicating to host.", e);
            }
        }

        public IContainer BuildContainer()
        {
            var container = ContainerBuilder.Build();

            logger.Debug("Initializing Components");

            container.Resolve<DatabaseTarget>().Register();
            LogConfiguration.Reload();

            RegisterReporting(container);
            RegisterQuality(container);

            var indexers = container.Resolve<IEnumerable<IndexerBase>>();
            container.Resolve<IndexerProvider>().InitializeIndexers(indexers.ToList());

            var newznabIndexers = new List<NewznabDefinition>
                                      {
                                              new NewznabDefinition { Enable = false, Name = "Nzbs.org", Url = "http://nzbs.org", BuiltIn = true },
                                              new NewznabDefinition { Enable = false, Name = "Nzb.su", Url = "https://nzb.su", BuiltIn = true },
                                              new NewznabDefinition { Enable = false, Name = "Dognzb.cr", Url = "https://dognzb.cr", BuiltIn = true }
                                      };

            container.Resolve<NewznabProvider>().InitializeNewznabIndexers(newznabIndexers);

            container.Resolve<JobProvider>().Initialize();
            container.Resolve<WebTimer>().StartTimer(30);

            var notifiers = container.Resolve<IEnumerable<ExternalNotificationBase>>();
            container.Resolve<ExternalNotificationProvider>().InitializeNotifiers(notifiers.ToList());

            var providers = container.Resolve<IEnumerable<MetadataBase>>();
            container.Resolve<MetadataProvider>().Initialize(providers.ToList());

            //SignalR
            GlobalHost.DependencyResolver = new AutofacSignalrDependencyResolver(container.BeginLifetimeScope("SignalR"));

            return container;
        }

        private static void ShutDown()
        {
            logger.Info("Shutting down application...");
            WebTimer.Stop();
            Process.GetCurrentProcess().Kill();
        }
    }
}
