using System;
using System.Threading;
using NLog;
using NzbDrone.Common;
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
                var container = Bootstrap.Start(startupArgs, new ConsoleAlerts());

                if (startupArgs.InstallService || startupArgs.UninstallService)
                {
                    return;
                }

                var serviceFactory = container.Resolve<INzbDroneServiceFactory>();
                    
                while (!serviceFactory.IsServiceStopped)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (TerminateApplicationException)
            {
            }
            catch (Exception e)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                Logger.FatalException("EPIC FAIL!", e);
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadLine();
            }
        }
    }
}
