using System;
using System.Diagnostics;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Jobs;
using PetaPoco;
using LogConfiguration = NzbDrone.Core.Instrumentation.LogConfiguration;

namespace NzbDrone.Core
{
    public class CentralDispatch
    {
        private readonly Object KernelLock = new object();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CentralDispatch()
        {
            InitializeApp();
        }

        public StandardKernel Kernel { get; private set; }

        private void InitializeApp()
        {
            BindKernel();

            Kernel.Get<LogConfiguration>().Setup();

            var mainConnectionString = Kernel.Get<Connection>().MainConnectionString;

            MigrationsHelper.Run(mainConnectionString, true);

            LogConfiguration.RegisterDatabaseLogger(Kernel.Get<DatabaseTarget>());
            LogConfiguration.Reload();

            Kernel.Get<QualityProvider>().SetupDefaultProfiles();
            Kernel.Get<QualityTypeProvider>().SetupDefault();
            Kernel.Get<ConfigFileProvider>().CreateDefaultConfigFile();

            BindExternalNotifications();
            BindIndexers();
            BindJobs();
        }

        private void BindKernel()
        {
            lock (KernelLock)
            {
                Logger.Debug("Binding Ninject's Kernel");
                Kernel = new StandardKernel();

                var connection = Kernel.Get<Connection>();

                Kernel.Bind<IDatabase>().ToMethod(c => connection.GetMainPetaPocoDb()).InTransientScope();
                Kernel.Bind<IDatabase>().ToMethod(c => connection.GetLogPetaPocoDb(false)).WhenInjectedInto<DatabaseTarget>().InSingletonScope();
                Kernel.Bind<IDatabase>().ToMethod(c => connection.GetLogPetaPocoDb()).WhenInjectedInto<LogProvider>().InSingletonScope();

                Kernel.Bind<JobProvider>().ToSelf().InSingletonScope();
            }
        }

        private void BindIndexers()
        {
            Kernel.Bind<IndexerBase>().To<NzbsOrg>();
            Kernel.Bind<IndexerBase>().To<NzbMatrix>();
            Kernel.Bind<IndexerBase>().To<NzbsRUs>();
            Kernel.Bind<IndexerBase>().To<Newzbin>();

            var indexers = Kernel.GetAll<IndexerBase>();
            Kernel.Get<IndexerProvider>().InitializeIndexers(indexers.ToList());
        }

        private void BindJobs()
        {
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

            Kernel.Get<JobProvider>().Initialize();
            Kernel.Get<WebTimer>().StartTimer(30);
        }

        private void BindExternalNotifications()
        {
            Kernel.Bind<ExternalNotificationBase>().To<Xbmc>();
            Kernel.Bind<ExternalNotificationBase>().To<Smtp>();
            Kernel.Bind<ExternalNotificationBase>().To<Twitter>();
            Kernel.Bind<ExternalNotificationBase>().To<Providers.ExternalNotification.Growl>();
            Kernel.Bind<ExternalNotificationBase>().To<Prowl>();

            var notifiers = Kernel.GetAll<ExternalNotificationBase>();
            Kernel.Get<ExternalNotificationProvider>().InitializeNotifiers(notifiers.ToList());
        }

        /// <summary>
        ///   Forces IISExpress process to exit with the host application
        /// </summary>
        public void DedicateToHost()
        {
            try
            {
                var pid = Convert.ToInt32(Environment.GetEnvironmentVariable("NZBDRONE_PID"));

                Logger.Debug("Attaching to parent process ({0}) for automatic termination.", pid);

                var hostProcess = Process.GetProcessById(Convert.ToInt32(pid));

                hostProcess.EnableRaisingEvents = true;
                hostProcess.Exited += (delegate
                                           {
                                               Logger.Info("Host has been terminated. Shutting down web server.");
                                               ShutDown();
                                           });

                Logger.Debug("Successfully Attached to host. Process [{0}]", hostProcess.ProcessName);
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
        }

        private static void ShutDown()
        {
            Logger.Info("Shutting down application.");
            WebTimer.Stop();
            Process.GetCurrentProcess().Kill();
        }
    }
}
