using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NzbDrone.Common;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NzbDrone.Web.App_Start.Logging), "PreStart")]

namespace NzbDrone.Web.App_Start
{

    public static class Logging
    {
        public static void PreStart()
        {
            var environmentProvider = new EnvironmentProvider();

            LogManager.Configuration = new XmlLoggingConfiguration(environmentProvider.GetNlogConfigPath(), false);

            LogConfiguration.RegisterUdpLogger();
            LogConfiguration.RegisterConsoleLogger(LogLevel.Info, "NzbDrone.Web.MvcApplication");
            LogConfiguration.RegisterConsoleLogger(LogLevel.Info, "NzbDrone.Core.CentralDispatch");
            LogConfiguration.RegisterRollingFileLogger(environmentProvider.GetLogFileName(), LogLevel.Trace);

        }
    }
}