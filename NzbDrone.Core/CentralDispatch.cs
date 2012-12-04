using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeskMetrics;
using Ninject;
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
using SignalR.Hosting.AspNet;
using SignalR.Infrastructure;
using SignalR.Ninject;
using Connection = NzbDrone.Core.Datastore.Connection;
using Xbmc = NzbDrone.Core.Providers.ExternalNotification.Xbmc;

namespace NzbDrone.Core
{
    public class CentralDispatch
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly EnvironmentProvider _environmentProvider;

        public StandardKernel Kernel { get; private set; }

        public CentralDispatch()
        {
            _environmentProvider = new EnvironmentProvider();

            logger.Debug("Initializing Kernel:");
            Kernel = new StandardKernel();

            GlobalHost.DependencyResolver = new NinjectDependencyResolver(Kernel);

            InitDatabase();
            InitReporting();

            InitQuality();
            InitExternalNotifications();
            InitMetadataProviders();
            InitIndexers();
            InitJobs();         
        }

        private void InitDatabase()
        {
            logger.Info("Initializing Database...");

            var appDataPath = _environmentProvider.GetAppDataPath();
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            var connection = Kernel.Get<Connection>();
            Kernel.Bind<IDatabase>().ToMethod(c => connection.GetMainPetaPocoDb()).InTransientScope();
            Kernel.Bind<IDatabase>().ToMethod(c => connection.GetLogPetaPocoDb(false)).WhenInjectedInto<DatabaseTarget>().InSingletonScope();
            Kernel.Bind<IDatabase>().ToMethod(c => connection.GetLogPetaPocoDb()).WhenInjectedInto<LogProvider>();
            Kernel.Bind<LogDbContext>().ToMethod(c => connection.GetLogEfContext()).WhenInjectedInto<LogProvider>().InSingletonScope();

            Kernel.Get<DatabaseTarget>().Register();
            LogConfiguration.Reload();
        }

        private void InitReporting()
        {
            EnvironmentProvider.UGuid = Kernel.Get<ConfigProvider>().UGuid;
            ReportingService.RestProvider = Kernel.Get<RestProvider>();
            ReportingService.SetupExceptronDriver();
        }

        private void InitQuality()
        {
            logger.Debug("Initializing Quality...");
            Kernel.Get<QualityProvider>().SetupDefaultProfiles();
            Kernel.Get<QualityTypeProvider>().SetupDefault();
        }

        private void InitIndexers()
        {
            logger.Debug("Initializing Indexers...");
            Kernel.Bind<IndexerBase>().To<NzbMatrix>();
            Kernel.Bind<IndexerBase>().To<NzbsRUs>();
            Kernel.Bind<IndexerBase>().To<Newznab>();
            Kernel.Bind<IndexerBase>().To<Wombles>();
            Kernel.Bind<IndexerBase>().To<FileSharingTalk>();
            Kernel.Bind<IndexerBase>().To<NzbIndex>();
            Kernel.Bind<IndexerBase>().To<NzbClub>();

            var indexers = Kernel.GetAll<IndexerBase>();
            Kernel.Get<IndexerProvider>().InitializeIndexers(indexers.ToList());

            var newznabIndexers = new List<NewznabDefinition>
                                      {
                                              new NewznabDefinition { Enable = false, Name = "Nzbs.org", Url = "http://nzbs.org", BuiltIn = true },
                                              new NewznabDefinition { Enable = false, Name = "Nzb.su", Url = "https://nzb.su", BuiltIn = true },
                                              new NewznabDefinition { Enable = false, Name = "Dognzb.cr", Url = "https://dognzb.cr", BuiltIn = true }
                                      };

            Kernel.Get<NewznabProvider>().InitializeNewznabIndexers(newznabIndexers);
        }

        private void InitJobs()
        {
            logger.Debug("Initializing Background Jobs...");

            Kernel.Bind<JobProvider>().ToSelf().InSingletonScope();

            Kernel.Bind<IJob>().To<RssSyncJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<ImportNewSeriesJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<UpdateInfoJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<DiskScanJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<DeleteSeriesJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<EpisodeSearchJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<PostDownloadScanJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<UpdateSceneMappingsJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<SeasonSearchJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<RenameSeasonJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<SeriesSearchJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<RenameSeriesJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<BacklogSearchJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<BannerDownloadJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<ConvertEpisodeJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<AppUpdateJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<TrimLogsJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<RecentBacklogSearchJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<SearchHistoryCleanupJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<PastWeekBacklogSearchJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<RefreshEpisodeMetadata>().InSingletonScope();
            Kernel.Bind<IJob>().To<CleanupRecycleBinJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<EmptyRecycleBinJob>().InSingletonScope();
            Kernel.Bind<IJob>().To<XemUpdateJob>().InSingletonScope();

            Kernel.Get<JobProvider>().Initialize();
            Kernel.Get<WebTimer>().StartTimer(30);
        }

        private void InitExternalNotifications()
        {
            logger.Debug("Initializing External Notifications...");
            Kernel.Bind<ExternalNotificationBase>().To<Xbmc>();
            Kernel.Bind<ExternalNotificationBase>().To<Smtp>();
            Kernel.Bind<ExternalNotificationBase>().To<Twitter>();
            Kernel.Bind<ExternalNotificationBase>().To<Providers.ExternalNotification.Growl>();
            Kernel.Bind<ExternalNotificationBase>().To<Prowl>();
            Kernel.Bind<ExternalNotificationBase>().To<Plex>();

            var notifiers = Kernel.GetAll<ExternalNotificationBase>();
            Kernel.Get<ExternalNotificationProvider>().InitializeNotifiers(notifiers.ToList());
        }

        private void InitMetadataProviders()
        {
            logger.Debug("Initializing Metadata Providers...");

            Kernel.Bind<MetadataBase>().To<Providers.Metadata.Xbmc>().InSingletonScope();

            var providers = Kernel.GetAll<MetadataBase>();
            Kernel.Get<MetadataProvider>().Initialize(providers.ToList());
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

        private static void ShutDown()
        {
            logger.Info("Shutting down application...");
            WebTimer.Stop();
            Process.GetCurrentProcess().Kill();
        }
    }
}
