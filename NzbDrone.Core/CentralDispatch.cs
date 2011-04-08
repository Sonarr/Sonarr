using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Ninject;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Fakes;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.DataProviders;
using SubSonic.Query;
using SubSonic.Repository;
using NLog;
using System.Linq;

namespace NzbDrone.Core
{
    public static class CentralDispatch
    {
        private static StandardKernel _kernel;
        private static readonly Object kernelLock = new object();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static string _startupPath;

        public static void BindKernel()
        {
            lock (kernelLock)
            {
                Logger.Debug("Binding Ninject's Kernel");
                _kernel = new StandardKernel();

                //Store the startup path 
                _startupPath = AppPath;

                //Sqlite
                var AppDataPath = new DirectoryInfo(Path.Combine(AppPath, "App_Data", "nzbdrone.db"));
                if (!AppDataPath.Exists) AppDataPath.Create();

                string connectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppDataPath.FullName, "nzbdrone.db"));
                var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");

                string logConnectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppDataPath.FullName, "log.db"));
                var logDbProvider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SQLite");


                //SQLExpress
                //string logConnectionString = String.Format(@"server=.\SQLExpress; database=NzbDroneLogs; Trusted_Connection=True;");
                //var logDbProvider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SqlClient");
                var logRepository = new SimpleRepository(logDbProvider, SimpleRepositoryOptions.RunMigrations);
                //dbProvider.ExecuteQuery(new QueryCommand("VACUUM", dbProvider));

                dbProvider.Log = new NlogWriter();

                _kernel.Bind<ISeriesProvider>().To<SeriesProvider>().InSingletonScope();
                _kernel.Bind<IRssSyncProvider>().To<RssSyncProvider>().InSingletonScope();
                _kernel.Bind<ISeasonProvider>().To<SeasonProvider>();
                _kernel.Bind<IEpisodeProvider>().To<EpisodeProvider>();
                _kernel.Bind<IUpcomingEpisodesProvider>().To<UpcomingEpisodesProvider>();
                _kernel.Bind<IDiskProvider>().To<DiskProvider>();
                _kernel.Bind<TvDbProvider>().To<TvDbProvider>();
                _kernel.Bind<IDownloadProvider>().To<SabProvider>();
                _kernel.Bind<HttpProvider>().To<HttpProvider>();
                _kernel.Bind<IHistoryProvider>().To<HistoryProvider>();
                _kernel.Bind<IQualityProvider>().To<QualityProvider>();
                _kernel.Bind<IRootDirProvider>().To<RootDirProvider>();
                _kernel.Bind<IExtenalNotificationProvider>().To<ExternalNotificationProvider>();
                _kernel.Bind<IXbmcProvider>().To<XbmcProvider>();
                _kernel.Bind<IConfigProvider>().To<ConfigProvider>().InSingletonScope();
                _kernel.Bind<ISyncProvider>().To<SyncProvider>().InSingletonScope();
                _kernel.Bind<IIndexerProvider>().To<IndexerProvider>().InSingletonScope();
                _kernel.Bind<IRenameProvider>().To<RenameProvider>().InSingletonScope();
                _kernel.Bind<INotificationProvider>().To<NotificationProvider>().InSingletonScope();
                _kernel.Bind<ILogProvider>().To<LogProvider>().InSingletonScope();
                _kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>().InSingletonScope();
                _kernel.Bind<ITimerProvider>().To<TimerProvider>().InSingletonScope();
                _kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations)).InSingletonScope();

                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<SubsonicTarget>().InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<LogProvider>().InSingletonScope();

                ForceMigration(_kernel.Get<IRepository>());
                SetupIndexers(_kernel.Get<IRepository>()); //Setup the default set of indexers on start-up
                SetupDefaultQualityProfiles(_kernel.Get<IRepository>()); //Setup the default QualityProfiles on start-up

                //Get the Timers going
                var config = _kernel.Get<IConfigProvider>();
                var timer = _kernel.Get<ITimerProvider>();
                timer.SetRssSyncTimer(Convert.ToInt32(config.GetValue("SyncFrequency", "15", true)));
                timer.StartRssSyncTimer();
            }
        }

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

        public static string ExecutablePath
        {
            get
            {
                //var uri = new Uri(Assembly.EscapedCodeBase);
                //return Path.GetDirectoryName(uri.LocalPath);
                return Directory.GetCurrentDirectory();
            }
        }

        public static string StartupPath
        {
            get { return _startupPath; }
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

        private static void ForceMigration(IRepository repository)
        {
            repository.GetPaged<Series>(0, 1);
            repository.GetPaged<EpisodeFile>(0, 1);
            repository.GetPaged<Episode>(0, 1);
            repository.GetPaged<Season>(0, 1);
            repository.GetPaged<History>(0, 1);
            repository.GetPaged<Indexer>(0, 1);
        }

        /// <summary>
        /// This method forces IISExpress process to exit with the host application
        /// </summary>
        public static void DedicateToHost()
        {
            try
            {
                Logger.Debug("Attaching to parent process for automatic termination.");
                var pc = new PerformanceCounter("Process", "Creating Process ID", Process.GetCurrentProcess().ProcessName);
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

        private static void SetupIndexers(IRepository repository)
        {
            //Setup the default providers in the Providers table

            string nzbMatrixRss = "http://rss.nzbmatrix.com/rss.php?page=download&username={USERNAME}&apikey={APIKEY}&subcat=6,41&english=1";
            string nzbMatrixApi = "http://rss.nzbmatrix.com/rss.php?page=download&username={USERNAME}&apikey={APIKEY}&subcat=6,41&english=1&age={AGE}&term={TERM}";
            string nzbsOrgRss = "http://nzbs.org/rss.php?type=1&dl=1&num=100&i={UID}&h={HASH}";
            string nzbsOrgApi = String.Empty;
            string nzbsrusRss = "http://www.nzbsrus.com/rssfeed.php?cat=91,75&i={UID}&h={HASH}";
            string nzbsrusApi = String.Empty;

            var nzbMatrixIndexer = new Indexer
                                       {
                                           IndexerId = 1,
                                           IndexerName = "NzbMatrix",
                                           RssUrl = nzbMatrixRss,
                                           ApiUrl = nzbMatrixApi,
                                           Order = 1
                                       };

            var nzbsOrgIndexer = new Indexer
                                     {
                                         IndexerId = 2,
                                         IndexerName = "NzbsOrg",
                                         RssUrl = nzbsOrgRss,
                                         ApiUrl = nzbsOrgApi,
                                         Order = 2
                                     };

            var nzbsrusIndexer = new Indexer
                              {
                                  IndexerId = 3,
                                  IndexerName = "Nzbsrus",
                                  RssUrl = nzbsrusRss,
                                  ApiUrl = nzbsrusApi,
                                  Order = 3
                              };

            //NzbMatrix
            Logger.Debug("Checking for NzbMatrix Indexer");
            var nzbMatix = repository.Single<Indexer>(1);
            if (nzbMatix == null)
            {
                Logger.Debug("Adding new Indexer: NzbMatrix");
                repository.Add(nzbMatrixIndexer);
            }

            else
            {
                Logger.Debug("Updating Indexer: NzbMatrix");
                nzbMatix.RssUrl = nzbMatrixIndexer.RssUrl;
                nzbMatix.ApiUrl = nzbMatrixIndexer.ApiUrl;
                repository.Update(nzbMatix);
            }

            //Nzbs.org
            Logger.Debug("Checking for Nzbs.org");
            var nzbsOrg = repository.Single<Indexer>(2);
            if (nzbsOrg == null)
            {
                Logger.Debug("Adding new Indexer: Nzbs.org");
                repository.Add(nzbsOrgIndexer);
            }

            else
            {
                Logger.Debug("Updating Indexer: Nzbs.org");
                nzbsOrg.RssUrl = nzbsOrgIndexer.RssUrl;
                nzbsOrg.ApiUrl = nzbsOrgIndexer.ApiUrl;
                repository.Update(nzbsOrg);
            }

            //Nzbsrus
            Logger.Debug("Checking for Nzbsrus");
            var nzbsrus = repository.Single<Indexer>(3);
            if (nzbsrus == null)
            {
                Logger.Debug("Adding new Indexer: Nzbsrus");
                repository.Add(nzbsrusIndexer);
            }

            else
            {
                Logger.Debug("Updating Indexer: Nzbsrus");
                nzbsrus.RssUrl = nzbsOrgIndexer.RssUrl;
                nzbsrus.ApiUrl = nzbsOrgIndexer.ApiUrl;
                repository.Update(nzbsrus);
            }
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
                                 Allowed = new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.WEBDL, QualityTypes.BDRip, QualityTypes.Bluray720 },
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