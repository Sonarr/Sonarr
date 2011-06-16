using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
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

            LogConfiguration.StartDbLogging();

            MigrationsHelper.Run(Connection.MainConnectionString, true);

            _kernel.Get<QualityProvider>().SetupDefaultProfiles();

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

                _kernel.Bind<IDatabase>().ToMethod(c => Connection.GetPetaPocoDb(Connection.MainConnectionString)).InRequestScope();
                _kernel.Bind<IRepository>().ToConstant(Connection.CreateSimpleRepository(Connection.MainConnectionString)).InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(Connection.CreateSimpleRepository(Connection.LogConnectionString)).WhenInjectedInto<SubsonicTarget>().InSingletonScope();
                _kernel.Bind<IRepository>().ToConstant(Connection.CreateSimpleRepository(Connection.LogConnectionString)).WhenInjectedInto<LogProvider>().InSingletonScope();
            }
        }

        private static void BindIndexers()
        {
            _kernel.Bind<IndexerBase>().To<NzbsOrg>().InTransientScope();
            _kernel.Bind<IndexerBase>().To<NzbMatrix>().InTransientScope();
            _kernel.Bind<IndexerBase>().To<NzbsRUs>().InTransientScope();
            _kernel.Bind<IndexerBase>().To<Newzbin>().InTransientScope();

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
            _kernel.Bind<IJob>().To<EpisodeSearchJob>().InTransientScope();
            _kernel.Bind<IJob>().To<RenameEpisodeJob>().InTransientScope();
            _kernel.Bind<IJob>().To<PostDownloadScanJob>().InTransientScope();

            _kernel.Get<JobProvider>().Initialize();
            _kernel.Get<WebTimer>().StartTimer(30);
        }

        private static void BindExternalNotifications()
        {
            _kernel.Bind<ExternalNotificationProviderBase>().To<XbmcNotificationProvider>().InSingletonScope();
            var notifiers = _kernel.GetAll<ExternalNotificationProviderBase>();
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