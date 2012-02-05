using System.Linq;
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
            LogConfiguration.RegisterUdpLogger();
            LogConfiguration.RegisterFileLogger("${basedir}/_logs/${shortdate}.log", LogLevel.Trace);
            LogConfiguration.Reload();

            logger.Info("Logger has been configured. (App Start)");

           
        }
    }
}