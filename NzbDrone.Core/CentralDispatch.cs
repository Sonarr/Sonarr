using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using Ninject;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers;
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
        private static IKernel _kernel;
        private static readonly Object kernelLock = new object();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void BindKernel()
        {
            lock (kernelLock)
            {
                Logger.Debug("Binding Ninject's Kernel");
                _kernel = new StandardKernel();

                //Sqlite
                string connectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppPath, "nzbdrone.db"));
                var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");

                //SQLExpress
                //string connectionString = String.Format(@"server=.\SQLExpress; database=NzbDrone; Trusted_Connection=True;");
                //var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SqlClient");

                //Sqlite
                string logConnectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppPath, "log.db"));
                var logDbProvider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SQLite");

                //SQLExpress
                //string logConnectionString = String.Format(@"server=.\SQLExpress; database=NzbDroneLogs; Trusted_Connection=True;");
                //var logDbProvider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SqlClient");
                var logRepository = new SimpleRepository(logDbProvider, SimpleRepositoryOptions.RunMigrations);
                //dbProvider.ExecuteQuery(new QueryCommand("VACUUM", dbProvider));

                dbProvider.Log = new NlogWriter();
                dbProvider.LogParams = true;

                _kernel.Bind<ISeriesProvider>().To<SeriesProvider>().InSingletonScope();
                _kernel.Bind<ISeasonProvider>().To<SeasonProvider>();
                _kernel.Bind<IEpisodeProvider>().To<EpisodeProvider>();
                _kernel.Bind<IDiskProvider>().To<DiskProvider>();
                _kernel.Bind<ITvDbProvider>().To<TvDbProvider>();
                _kernel.Bind<IDownloadProvider>().To<SabProvider>();
                _kernel.Bind<IHttpProvider>().To<HttpProvider>();
                _kernel.Bind<IHistoryProvider>().To<HistoryProvider>();
                _kernel.Bind<IQualityProvider>().To<QualityProvider>();
                _kernel.Bind<IRootDirProvider>().To<RootDirProvider>();
                _kernel.Bind<IExtenalNotificationProvider>().To<ExternalNotificationProvider>();
                _kernel.Bind<IXbmcProvider>().To<XbmcProvider>();
                _kernel.Bind<IConfigProvider>().To<ConfigProvider>().InSingletonScope();
                _kernel.Bind<ISyncProvider>().To<SyncProvider>().InSingletonScope();
                _kernel.Bind<IRssProvider>().To<RssProvider>().InSingletonScope();
                _kernel.Bind<IRssSyncProvider>().To<RssSyncProvider>().InSingletonScope();
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
                if (HttpContext.Current != null)
                {
                    return new DirectoryInfo(HttpContext.Current.Server.MapPath("\\")).FullName;
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

        public static IKernel NinjectKernel
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
                                           IndexerName = "NzbMatrix",
                                           RssUrl = nzbMatrixRss,
                                           ApiUrl = nzbMatrixApi,
                                           Order = 1
                                       };

            var nzbsOrgIndexer = new Indexer
                                     {
                                         IndexerName = "NzbsOrg",
                                         RssUrl = nzbsOrgRss,
                                         ApiUrl = nzbsOrgApi,
                                         Order = 2
                                     };

            var nzbsrusIndexer = new Indexer
                              {
                                  IndexerName = "Nzbsrus",
                                  RssUrl = nzbsrusRss,
                                  ApiUrl = nzbsrusApi,
                                  Order = 3
                              };

            //NzbMatrix
            Logger.Debug("Checking for NzbMatrix Indexer");
            var nzbMatix = repository.Single<Indexer>("NzbMatrix");
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
            var nzbsOrg = repository.Single<Indexer>("NzbsOrg");
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
            var nzbsrus = repository.Single<Indexer>("Nzbsrus");
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
            var sdtv = new QualityProfile
                            {
                                Name = "SDTV",
                                Allowed = new List<QualityTypes> { QualityTypes.TV },
                                Cutoff = QualityTypes.TV
                            };

            var dvd = new QualityProfile
                             {
                                 Name = "DVD SD",
                                 Allowed = new List<QualityTypes> { QualityTypes.DVD },
                                 Cutoff = QualityTypes.DVD
                             };

            var bdrip = new QualityProfile
                             {
                                 Name = "BDRip",
                                 Allowed = new List<QualityTypes> { QualityTypes.BDRip },
                                 Cutoff = QualityTypes.BDRip
                             };

            var hdtv = new QualityProfile
                             {
                                 Name = "HDTV",
                                 Allowed = new List<QualityTypes> { QualityTypes.HDTV },
                                 Cutoff = QualityTypes.HDTV
                             };

            var webdl = new QualityProfile
                           {
                               Name = "WEBDL",
                               Allowed = new List<QualityTypes> { QualityTypes.WEBDL },
                               Cutoff = QualityTypes.WEBDL
                           };

            var bluray = new QualityProfile
                           {
                               Name = "Bluray",
                               Allowed = new List<QualityTypes> { QualityTypes.Bluray },
                               Cutoff = QualityTypes.Bluray
                           };

            //Add or Update SDTV
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", sdtv.Name));
            var sdtvDb = repository.Single<QualityProfile>(i => i.Name == sdtv.Name);
            if (sdtvDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", sdtv.Name));
                repository.Add(sdtv);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", sdtv.Name));
                sdtv.QualityProfileId = sdtvDb.QualityProfileId;
                repository.Update(sdtv);
            }

            //Add or Update DVD
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", dvd.Name));
            var dvdDb = repository.Single<QualityProfile>(i => i.Name == dvd.Name);
            if (dvdDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", dvd.Name));
                repository.Add(dvd);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", dvd.Name));
                dvd.QualityProfileId = dvdDb.QualityProfileId;
                repository.Update(dvd);
            }

            //Add or Update BDRip
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", bdrip.Name));
            var bdripDb = repository.Single<QualityProfile>(i => i.Name == bdrip.Name);
            if (bdripDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", bdrip.Name));
                repository.Add(bdrip);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", bdrip.Name));
                bdrip.QualityProfileId = bdripDb.QualityProfileId;
                repository.Update(bdrip);
            }

            //Add or Update HDTV
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", hdtv.Name));
            var hdtvDb = repository.Single<QualityProfile>(i => i.Name == hdtv.Name);
            if (hdtvDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", hdtv.Name));
                repository.Add(hdtv);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", hdtv.Name));
                hdtv.QualityProfileId = hdtvDb.QualityProfileId;
                repository.Update(hdtv);
            }

            //Add or Update WEBDL
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", webdl.Name));
            var webdlDb = repository.Single<QualityProfile>(i => i.Name == webdl.Name);
            if (webdlDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", webdl.Name));
                repository.Add(webdl);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", webdl.Name));
                webdl.QualityProfileId = webdlDb.QualityProfileId;
                repository.Update(webdl);
            }

            //Add or Update Bluray
            Logger.Debug(String.Format("Checking for default QualityProfile: {0}", bluray.Name));
            var blurayDb = repository.Single<QualityProfile>(i => i.Name == bluray.Name);
            if (blurayDb == null)
            {
                Logger.Debug(String.Format("Adding new default QualityProfile: {0}", bluray.Name));
                repository.Add(bluray);
            }

            else
            {
                Logger.Debug(String.Format("Updating default QualityProfile: {0}", bluray.Name));
                bluray.QualityProfileId = blurayDb.QualityProfileId;
                repository.Update(bluray);
            }
        }
    }
}