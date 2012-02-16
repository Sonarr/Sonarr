using System.Linq;
using System.Web.Hosting;
using NLog;
using NzbDrone.Common;
using NzbDrone.Services.Service.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Logging), "PreStart")]

namespace NzbDrone.Services.Service.App_Start
{

    public static class Logging
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void PreStart()
        {
            string logPath = string.Format("C:\\NLog\\{0}\\{1}\\${{shortdate}}.log", HostingEnvironment.SiteName, new EnviromentProvider().Version);

            LogConfiguration.RegisterUdpLogger();
            LogConfiguration.RegisterFileLogger(logPath, LogLevel.Trace);
            LogConfiguration.Reload();

            logger.Info("Logger has been configured. (App Start)");


        }
    }
}