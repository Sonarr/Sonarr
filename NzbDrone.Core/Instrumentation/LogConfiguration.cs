using System.IO;
using Ninject;
using NLog;
using NLog.Config;
using NzbDrone.Common;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Instrumentation
{
    public class LogConfiguration
    {
        private readonly EnviromentProvider _enviromentProvider;
        private readonly DatabaseTarget _databaseTarget;

        public LogConfiguration(EnviromentProvider enviromentProvider, DatabaseTarget databaseTarget)
        {
            _enviromentProvider = enviromentProvider;
            _databaseTarget = databaseTarget;
        }

        public void Setup()
        {
            if (Common.EnviromentProvider.IsProduction)
            {
                LogManager.ThrowExceptions = false;
            }

            LogManager.Configuration = new XmlLoggingConfiguration(_enviromentProvider.GetNlogConfigPath(), false);

            Common.LogConfiguration.RegisterConsoleLogger(LogLevel.Info, "NzbDrone.Web.MvcApplication");
            Common.LogConfiguration.RegisterConsoleLogger(LogLevel.Info, "NzbDrone.Core.CentralDispatch");

            LogManager.ConfigurationReloaded += ((s, e) => RegisterDatabaseLogger(_databaseTarget));
            
        }

        public static void RegisterDatabaseLogger(DatabaseTarget databaseTarget)
        {
            LogManager.Configuration.AddTarget("DbLogger", databaseTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, databaseTarget));
            Reload();
        }

        public static void Reload()
        {
            Common.LogConfiguration.Reload();
        }
    }
}