using System;
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
using PetaPoco;
using SignalR;
using SignalR.Hosting.AspNet;
using SignalR.Infrastructure;
using SignalR.Ninject;
using Connection = NzbDrone.Core.Datastore.Connection;

namespace NzbDrone.Core
{
    public class CentralDispatch
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly EnviromentProvider _enviromentProvider;

        public StandardKernel Kernel { get; private set; }

        public CentralDispatch()
        {
            _enviromentProvider = new EnviromentProvider();

            logger.Debug("Initializing Kernel:");
            Kernel = new StandardKernel();

            var resolver = new NinjectDependencyResolver(Kernel);
            AspNetHost.SetResolver(resolver);

            InitDatabase();
            InitReporting();

            InitQuality();
            InitExternalNotifications();
            InitIndexers();
            InitJobs();
        }


        private void InitDatabase()
        {
            logger.Info("Initializing Database...");

            var appDataPath = _enviromentProvider.GetAppDataPath();
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
            EnviromentProvider.UGuid = Kernel.Get<ConfigProvider>().UGuid;
            ReportingService.RestProvider = Kernel.Get<RestProvider>();

            var appId = AnalyticsProvider.DESKMETRICS_TEST_ID;

            if (EnviromentProvider.IsProduction)
                appId = AnalyticsProvider.DESKMETRICS_PRODUCTION_ID;

            var deskMetricsClient = new DeskMetricsClient(Kernel.Get<ConfigProvider>().UGuid.ToString(), appId, _enviromentProvider.Version);
            Kernel.Bind<IDeskMetricsClient>().ToConstant(deskMetricsClient);

            Kernel.Get<AnalyticsProvider>().Checkpoint();
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
            Kernel.Bind<IndexerBase>().To<NzbsOrg>();
            Kernel.Bind<IndexerBase>().To<NzbMatrix>();
            Kernel.Bind<IndexerBase>().To<NzbsRUs>();
            Kernel.Bind<IndexerBase>().To<Newzbin>();
            Kernel.Bind<IndexerBase>().To<Newznab>();

            var indexers = Kernel.GetAll<IndexerBase>();
            Kernel.Get<IndexerProvider>().InitializeIndexers(indexers.ToList());
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
            Kernel.Bind<IJob>().To<RenameEpisodeJob>().InSingletonScope();
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
            Kernel.Bind<IJob>().To<CheckpointJob>().InSingletonScope();

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

            var notifiers = Kernel.GetAll<ExternalNotificationBase>();
            Kernel.Get<ExternalNotificationProvider>().InitializeNotifiers(notifiers.ToList());
        }

        public void DedicateToHost()
        {
            try
            {
                var pid = _enviromentProvider.NzbDroneProcessIdFromEnviroment;

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
