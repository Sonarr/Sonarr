using System;
using System.IO;
using System.Web;
using Ninject;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Core.Entities;
using NzbDrone.Core.Entities.Episode;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Fakes;
using SubSonic.DataProviders;
using SubSonic.Repository;
using NLog;

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
                var provider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
                provider.Log = new SonicTrace();
                provider.LogParams = true;

                _kernel.Bind<ISeriesProvider>().To<SeriesProvider>().InSingletonScope();
                _kernel.Bind<ISeasonProvider>().To<SeasonProvider>();
                _kernel.Bind<IEpisodeProvider>().To<EpisodeProvider>();
                _kernel.Bind<IDiskProvider>().To<DiskProvider>();
                _kernel.Bind<ITvDbProvider>().To<TvDbProvider>();
                _kernel.Bind<IConfigProvider>().To<ConfigProvider>().InSingletonScope();
                _kernel.Bind<INotificationProvider>().To<NotificationProvider>().InSingletonScope();
                _kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations)).InSingletonScope();

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

        public static void ConfigureNlog()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            string callSight = "${callsite:className=false:fileName=true:includeSourcePath=false:methodName=true}";

            // Step 2. Create targets and add them to the configuration 
            var debuggerTarget = new DebuggerTarget
            {
                Layout = callSight + "- ${logger}: ${message}"
            };


            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = callSight + ": ${message}"
            };


            var fileTarget = new FileTarget
            {
                FileName = "${basedir}/test.log",
                Layout = "${message}"
            };

            config.AddTarget("debugger", debuggerTarget);
            config.AddTarget("console", consoleTarget);
            //config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            // Step 4. Define rules
            //LoggingRule fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            LoggingRule debugRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
            LoggingRule consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);

            //config.LoggingRules.Add(fileRule);
            config.LoggingRules.Add(debugRule);
            config.LoggingRules.Add(consoleRule);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }

        private static void ForceMigration(IRepository repository)
        {
            repository.GetPaged<Series>(0, 1);
            repository.GetPaged<EpisodeInfo>(0, 1);
        }
    }
}