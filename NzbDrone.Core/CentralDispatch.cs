using System;
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
                _kernel.Bind<IConfigProvider>().To<ConfigProvider>().InSingletonScope();
                _kernel.Bind<ISyncProvider>().To<SyncProvider>().InSingletonScope();
                _kernel.Bind<IRssProvider>().To<RssProvider>().InSingletonScope();
                _kernel.Bind<IRssSyncProvider>().To<RssSyncProvider>().InSingletonScope();
                _kernel.Bind<IIndexerProvider>().To<IndexerProvider>().InSingletonScope();;
                _kernel.Bind<INotificationProvider>().To<NotificationProvider>().InSingletonScope();
                _kernel.Bind<ILogProvider>().To<LogProvider>().InSingletonScope();
                _kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>().InSingletonScope();
                _kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations)).InSingletonScope();

                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<SubsonicTarget>().InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<LogProvider>().InSingletonScope();


                ForceMigration(_kernel.Get<IRepository>());
                SetupIndexers(_kernel.Get<IRepository>()); //Setup the default set of indexers on start-up
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

            string nzbMatrixRss = "http://rss.nzbmatrix.com/rss.php?page=download&username={USERNAME}&apikey={APIKEY}&subcat=6&english=1";
            string nzbsOrgRss = "http://nzbs.org/rss.php?type=1&dl=1&num=100&i={UID}&h={HASH}";
            string nzbsrusRss = "http://www.nzbsrus.com/rssfeed.php?cat=91,75&i={UID}&h={HASH}";

            var nzbMatrixIndexer = new Indexer
                                       {
                                           IndexerName = "NzbMatrix",
                                           RssUrl = nzbMatrixRss,
                                           ApiUrl = String.Empty,
                                           Enabled = false,
                                           Order = 1
                                       };

            var nzbsOrgIndexer = new Indexer
                                     {
                                         IndexerName = "NzbsOrg",
                                         RssUrl = nzbsOrgRss,
                                         ApiUrl = String.Empty,
                                         Enabled = false,
                                         Order = 2
                                     };

            var nzbsrusIndexer = new Indexer
                              {
                                  IndexerName = "Nzbsrus",
                                  RssUrl = nzbsrusRss,
                                  ApiUrl = String.Empty,
                                  Enabled = false,
                                  Order = 3
                              };

            //NzbMatrix
            Logger.Debug("Checking for NzbMatrix Indexer");
            if (!repository.Exists<Indexer>(i => i.IndexerName == "NzbMatrix"))
            {
                Logger.Debug("Adding new Indexer: NzbMatrix");
                repository.Add(nzbMatrixIndexer);
            }

            else
            {
                Logger.Debug("Updating Indexer: NzbMatrix");
                repository.Update(nzbMatrixIndexer);
            }

            //Nzbs.org
            Logger.Debug("Checking for Nzbs.org");
            if (!repository.Exists<Indexer>(i => i.IndexerName == "NzbsOrg"))
            {
                Logger.Debug("Adding new Indexer: Nzbs.org");
                repository.Add(nzbsOrgIndexer);
            }

            else
            {
                Logger.Debug("Updating Indexer: Nzbs.org");
                repository.Update(nzbsOrgIndexer);
            }

            //Nzbsrus
            Logger.Debug("Checking for Nzbsrus");
            if (!repository.Exists<Indexer>(i => i.IndexerName == "Nzbsrus"))
            {
                Logger.Debug("Adding new Indexer: Nzbsrus");
                repository.Add(nzbsrusIndexer);
            }

            else
            {
                Logger.Debug("Updating Indexer: Nzbsrus");
                repository.Update(nzbsrusIndexer);
            }
        }
    }
}