using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Hosting;
using Ninject;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.DataProviders;
using SubSonic.Repository;

namespace NzbDrone.Core
{
    public static class CentralDispatch
    {
        private static StandardKernel _kernel;
        private static readonly Object KernelLock = new object();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static String AppPath
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(HostingEnvironment.ApplicationPhysicalPath))
                {
                    return HostingEnvironment.ApplicationPhysicalPath;
                }
                return Directory.GetCurrentDirectory();
            }
        }

        public static StandardKernel NinjectKernel
        {
            get
            {
                if (_kernel == null)
                {
                    InitializeApp();
                }
                return _kernel;
            }
        }

        private static void InitializeApp()
        {
            BindKernel();
            
            LogConfiguration.Setup();
            
            Migrations.Run();
            ForceMigration(_kernel.Get<IRepository>());
            
            SetupDefaultQualityProfiles(_kernel.Get<IRepository>()); //Setup the default QualityProfiles on start-up

            BindIndexers();
            BindJobs();
            BindExternalNotifications();
        }

        public static void BindKernel()
        {
            lock (KernelLock)
            {
                Logger.Debug("Binding Ninject's Kernel");
                _kernel = new StandardKernel();

                //dbProvider.Log = new NlogWriter();
                _kernel.Bind<QualityProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<TvDbProvider>().ToSelf().InTransientScope();
                _kernel.Bind<HttpProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<SeriesProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<SeasonProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<EpisodeProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<UpcomingEpisodesProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<DiskProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<SabProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<HistoryProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<RootDirProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<ExternalNotificationProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<XbmcProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<ConfigProvider>().To<ConfigProvider>().InSingletonScope();
                _kernel.Bind<SyncProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<RenameProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<NotificationProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<LogProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<MediaFileProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<JobProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<IndexerProvider>().ToSelf().InSingletonScope();
                _kernel.Bind<WebTimer>().ToSelf().InSingletonScope();
                _kernel.Bind<AutoConfigureProvider>().ToSelf().InSingletonScope();

                _kernel.Bind<IRepository>().ToConstant(Connection.MainDataRepository).InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(Connection.LogDataRepository).WhenInjectedInto<SubsonicTarget>().InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(Connection.LogDataRepository).WhenInjectedInto<LogProvider>().InSingletonScope();
            }
        }

        private static void BindIndexers()
        {
            _kernel.Bind<IndexerBase>().To<NzbsOrg>().InSingletonScope();
            _kernel.Bind<IndexerBase>().To<NzbMatrix>().InSingletonScope();
            _kernel.Bind<IndexerBase>().To<NzbsRUs>().InSingletonScope();
            _kernel.Bind<IndexerBase>().To<Newzbin>().InSingletonScope();
            var indexers = _kernel.GetAll<IndexerBase>();
            _kernel.Get<IndexerProvider>().InitializeIndexers(indexers.ToList());
        }

        private static void BindJobs()
        {
            _kernel.Bind<IJob>().To<RssSyncJob>().InTransientScope();
            _kernel.Bind<IJob>().To<ImportNewSeriesJob>().InTransientScope();
            _kernel.Bind<IJob>().To<UpdateInfoJob>().InTransientScope();
            _kernel.Bind<IJob>().To<DiskScanJob>().InTransientScope();
            _kernel.Bind<IJob>().To<DeleteSeriesJob>().InTransientScope();

            _kernel.Get<JobProvider>().Initialize();
            _kernel.Get<WebTimer>().StartTimer(30);
        }

        private static void BindExternalNotifications()
        {
            _kernel.Bind<ExternalNotificationProviderBase>().To<XbmcNotificationProvider>().InSingletonScope();
            var notifiers = _kernel.GetAll<ExternalNotificationProviderBase>();
            _kernel.Get<ExternalNotificationProvider>().InitializeNotifiers(notifiers.ToList());
        }

        private static void ForceMigration(IRepository repository)
        {
            repository.All<Series>().Count();
            repository.All<Season>().Count();
            repository.All<Episode>().Count();
            repository.All<EpisodeFile>().Count();
            repository.All<QualityProfile>().Count();
            repository.All<History>().Count();
            repository.All<IndexerSetting>().Count();
        }

        /// <summary>
        ///   Forces IISExpress process to exit with the host application
        /// </summary>
        public static void DedicateToHost()
        {
            try
            {
                Logger.Debug("Attaching to parent process for automatic termination.");
                var pc = new PerformanceCounter("Process", "Creating Process ID",
                                                Process.GetCurrentProcess().ProcessName);
                var pid = (int)pc.NextValue();
                var hostProcess = Process.GetProcessById(pid);

                hostProcess.EnableRaisingEvents = true;
                hostProcess.Exited += (delegate
                                           {
                                               Logger.Info("Host has been terminated. Shutting down web server.");
                                               ShutDown();
                                           });

                Logger.Debug("Successfully Attached to host. Process ID: {0}", pid);
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
        }

        private static void ShutDown()
        {
            Logger.Info("Shutting down application.");
            Process.GetCurrentProcess().Kill();
        }

        private static void SetupDefaultQualityProfiles(IRepository repository)
        {
            var sd = new QualityProfile
                         {
                             Name = "SD",
                             Allowed = new List<QualityTypes> { QualityTypes.SDTV, QualityTypes.DVD },
                             Cutoff = QualityTypes.SDTV
                         };

            var hd = new QualityProfile
                         {
                             Name = "HD",
                             Allowed =
                                 new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.WEBDL, QualityTypes.Bluray720p },
                             Cutoff = QualityTypes.HDTV
                         };

            //Add or Update SD
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", sd.Name));
            var sdDb = repository.Single<QualityProfile>(i => i.Name == sd.Name);
            if (sdDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", sd.Name));
                repository.Add(sd);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", sd.Name));
                sd.QualityProfileId = sdDb.QualityProfileId;
                repository.Update(sd);
            }

            //Add or Update HD
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", hd.Name));
            var hdDb = repository.Single<QualityProfile>(i => i.Name == hd.Name);
            if (hdDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", hd.Name));
                repository.Add(hd);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", hd.Name));
                hd.QualityProfileId = hdDb.QualityProfileId;
                repository.Update(hd);
            }
        }
    }
}