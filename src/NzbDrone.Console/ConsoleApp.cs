using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Host;

namespace NzbDrone.Console
{
    public static class ConsoleApp
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public static void Main(string[] args)
        {
            try
            {
                var startupArgs = new StartupContext(args);
                LogTargets.Register(startupArgs, false, true);
                Bootstrap.Start(startupArgs, new ConsoleAlerts());
            }
            catch (Exception e)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.FatalException("EPIC FAIL!", e);
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadLine();
                
                //Need this to terminate on mono (thanks nlog)
                LogManager.Configuration = null;
            }
        }
    }
}
