using System;
using System.IO;
using System.Web;
using Ninject;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Core.Providers;
using SubSonic.DataProviders;
using SubSonic.Repository;
using NLog;

namespace NzbDrone.Core
{
    public static class Main
    {
        public static void BindKernel(IKernel kernel)
        {
            string connectionString = String.Format("Data Source={0};Version=3;", Path.Combine(AppPath, "nzbdrone.db"));
            var provider = ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");

            kernel.Bind<ISeriesProvider>().To<SeriesProvider>();
            kernel.Bind<IDiskProvider>().To<DiskProvider>();
            kernel.Bind<ITvDbProvider>().To<TvDbProvider>();
            kernel.Bind<IConfigProvider>().To<ConfigProvider>();
            kernel.Bind<log4net.ILog>().ToMethod(c => log4net.LogManager.GetLogger("logger-name"));
            kernel.Bind<IRepository>().ToMethod(c => new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations));
        }

        public static String AppPath
        {
            get { return new DirectoryInfo(HttpContext.Current.Server.MapPath("\\")).FullName; }
        }


        public static void ConfigureNlog()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new DebuggerTarget();
            config.AddTarget("console", consoleTarget);

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = "${logger} ${message}";
            fileTarget.FileName = "${basedir}/test.log";
            fileTarget.Layout = "${message}";

            // Step 4. Define rules
            LoggingRule rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(rule1);

            LoggingRule rule2 = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            NLog.LogManager.Configuration = config;

            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Trace("trace log message");
            logger.Debug("debug log message");
            logger.Info("info log message");
            logger.Warn("warn log message");
            logger.Error("error log message");
            logger.Fatal("fatal log message");
        }
    }
}