using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Hosting;
using Ninject;
using NLog;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
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
                    BindKernel();
                }
                return _kernel;
            }
        }

        public static void BindKernel()
        {
            lock (KernelLock)
            {
                Logger.Debug("Binding Ninject's Kernel");
                _kernel = new StandardKernel();

                //Sqlite
                var appDataPath = new DirectoryInfo(Path.Combine(AppPath, "App_Data"));
                if (!appDataPath.Exists) appDataPath.Create();

                string connectionString = String.Format("Data Source={0};Version=3;",
                                                        Path.Combine(appDataPath.FullName, "nzbdrone.db"));
                var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");

                string logConnectionString = String.Format("Data Source={0};Version=3;",
                                                           Path.Combine(appDataPath.FullName, "log.db"));
                var logDbProvider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SQLite");


                //SQLExpress
                //string logConnectionString = String.Format(@"server=.\SQLExpress; database=NzbDroneLogs; Trusted_Connection=True;");
                //var logDbProvider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SqlClient");
                var logRepository = new SimpleRepository(logDbProvider, SimpleRepositoryOptions.RunMigrations);
                //dbProvider.ExecuteQuery(new QueryCommand("VACUUM", dbProvider));

                dbProvider.Log = new NlogWriter();

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
                _kernel.Bind<PostProcessingProvider>().ToSelf().InSingletonScope();
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
                _kernel.Bind<IRepository>().ToMethod(
                    c => new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations)).InSingletonScope();

                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<SubsonicTarget>().
                    InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<LogProvider>().InSingletonScope();

                ForceMigration(_kernel.Get<IRepository>());
                SetupDefaultQualityProfiles(_kernel.Get<IRepository>()); //Setup the default QualityProfiles on start-up

                BindIndexers();
                BindJobs();
            }
        }



        private static void BindIndexers()
        {
            _kernel.Bind<IndexerProviderBase>().To<NzbsOrgProvider>().InSingletonScope();
            _kernel.Bind<IndexerProviderBase>().To<NzbMatrixProvider>().InSingletonScope();
            _kernel.Bind<IndexerProviderBase>().To<NzbsRUsProvider>().InSingletonScope();
            _kernel.Bind<IndexerProviderBase>().To<NewzbinProvider>().InSingletonScope();
            var indexers = _kernel.GetAll<IndexerProviderBase>();
            _kernel.Get<IndexerProvider>().InitializeIndexers(indexers.ToList());
        }

        private static void BindJobs()
        {
            _kernel.Bind<IJob>().To<RssSyncJob>().InTransientScope();
            _kernel.Bind<IJob>().To<NewSeriesUpdate>().InTransientScope();
            _kernel.Bind<IJob>().To<UpdateInfoJob>().InTransientScope();

            _kernel.Get<JobProvider>().Initialize();
            _kernel.Get<WebTimer>().StartTimer(30);
        }


        private static void ForceMigration(IRepository repository)
        {
            repository.All<Series>().Count();
            repository.All<Season>().Count();
            repository.All<Episode>().Count();
            repository.All<EpisodeFile>().Count();
            repository.All<QualityProfile>().Count();
            repository.All<History>().Count();
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
                             Allowed = new List<QualityTypes> { QualityTypes.TV, QualityTypes.DVD },
                             Cutoff = QualityTypes.TV
                         };

            var hd = new QualityProfile
                         {
                             Name = "HD",
                             Allowed =
                                 new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.WEBDL, QualityTypes.BDRip, QualityTypes.Bluray720 },
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