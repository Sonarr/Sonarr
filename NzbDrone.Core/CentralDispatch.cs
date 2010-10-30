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

                string connectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppPath, "nzbdrone.db"));
                var dbProvider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
        
                string logConnectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppPath, "log.db"));
                var logDbProvider = ProviderFactory.GetProvider(logConnectionString, "System.Data.SQLite");
                var logRepository = new SimpleRepository(logDbProvider, SimpleRepositoryOptions.RunMigrations);
                //dbProvider.ExecuteQuery(new QueryCommand("VACUUM", dbProvider));

                dbProvider.Log = new NlogWriter();
                dbProvider.LogParams = true;

                _kernel.Bind<ISeriesProvider>().To<SeriesProvider>().InSingletonScope();
                _kernel.Bind<ISeasonProvider>().To<SeasonProvider>();
                _kernel.Bind<IEpisodeProvider>().To<EpisodeProvider>();
                _kernel.Bind<IDiskProvider>().To<DiskProvider>();
                _kernel.Bind<ITvDbProvider>().To<TvDbProvider>();
                _kernel.Bind<IConfigProvider>().To<ConfigProvider>().InSingletonScope();
                _kernel.Bind<ISyncProvider>().To<SyncProvider>().InSingletonScope();
                _kernel.Bind<INotificationProvider>().To<NotificationProvider>().InSingletonScope();
                _kernel.Bind<ILogProvider>().To<LogProvider>().InSingletonScope();
                _kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>().InSingletonScope();
                _kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(dbProvider, SimpleRepositoryOptions.RunMigrations)).InSingletonScope();

                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<SubsonicTarget>().InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(logRepository).WhenInjectedInto<LogProvider>().InSingletonScope();


                ForceMigration(_kernel.Get<IRepository>());
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
    }
}