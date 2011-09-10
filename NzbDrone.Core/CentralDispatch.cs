using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Ninject;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Jobs;
using PetaPoco;

namespace NzbDrone.Core
{
    public static class CentralDispatch
    {
        private static StandardKernel _kernel;
        private static readonly Object KernelLock = new object();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public static Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
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

        public static void InitializeApp()
        {
            BindKernel();

            MigrationsHelper.Run(Connection.MainConnectionString, true);
            
            LogConfiguration.StartDbLogging();

            _kernel.Get<QualityProvider>().SetupDefaultProfiles();

            BindExternalNotifications();
            BindIndexers();
            BindJobs();
        }

        private static void BindKernel()
        {
            lock (KernelLock)
            {
                Logger.Debug("Binding Ninject's Kernel");
                _kernel = new StandardKernel();

                _kernel.Bind<IDatabase>().ToMethod(c => Connection.GetPetaPocoDb(Connection.MainConnectionString)).InTransientScope();
                //_kernel.Bind<IDatabase>().ToMethod(c => Connection.GetPetaPocoDb(Connection.MainConnectionString, false)).WhenInjectedInto<IJob>();
                //_kernel.Bind<IDatabase>().ToMethod(c => Connection.GetPetaPocoDb(Connection.MainConnectionString, false)).WhenInjectedInto<JobProvider>();
                _kernel.Bind<IDatabase>().ToMethod(c => Connection.GetPetaPocoDb(Connection.LogConnectionString, false)).WhenInjectedInto<DatabaseTarget>().InSingletonScope();
                _kernel.Bind<IDatabase>().ToMethod(c => Connection.GetPetaPocoDb(Connection.LogConnectionString)).WhenInjectedInto<LogProvider>().InSingletonScope();
            }
        }

        private static void BindIndexers()
        {
            _kernel.Bind<IndexerBase>().To<NzbsOrg>();
            _kernel.Bind<IndexerBase>().To<NzbMatrix>();
            _kernel.Bind<IndexerBase>().To<NzbsRUs>();
            _kernel.Bind<IndexerBase>().To<Newzbin>();

            var indexers = _kernel.GetAll<IndexerBase>();
            _kernel.Get<IndexerProvider>().InitializeIndexers(indexers.ToList());
        }

        private static void BindJobs()
        {
            _kernel.Bind<IJob>().To<RssSyncJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<ImportNewSeriesJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<UpdateInfoJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<DiskScanJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<DeleteSeriesJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<EpisodeSearchJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<RenameEpisodeJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<PostDownloadScanJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<UpdateSceneMappingsJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<SeasonSearchJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<RenameSeasonJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<SeriesSearchJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<RenameSeriesJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<BacklogSearchJob>().InSingletonScope();
            _kernel.Bind<IJob>().To<BannerDownloadJob>().InSingletonScope();

            _kernel.Get<JobProvider>().Initialize();
            _kernel.Get<WebTimer>().StartTimer(30);
        }

        private static void BindExternalNotifications()
        {
            _kernel.Bind<ExternalNotificationBase>().To<Xbmc>();

            var notifiers = _kernel.GetAll<ExternalNotificationBase>();
            _kernel.Get<ExternalNotificationProvider>().InitializeNotifiers(notifiers.ToList());
        }

        /// <summary>
        ///   Forces IISExpress process to exit with the host application
        /// </summary>
        public static void DedicateToHost()
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
            Process.GetCurrentProcess().Kill();
        }
    }
}