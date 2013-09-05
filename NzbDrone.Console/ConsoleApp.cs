using System;
using System.Threading;
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
                var startupArgs = new StartupArguments(args);
                LogTargets.Register(startupArgs, false, true);
                Bootstrap.Start(startupArgs, new ConsoleAlerts());
            }
            catch (TerminateApplicationException)
            {
            }
            catch (Exception e)
            {
                Logger.FatalException("EPIC FAIL!", e);
                System.Console.ReadLine();
            }

            while (true)
            {
                Thread.Sleep(10 * 60);
            }
        }
    }
}
