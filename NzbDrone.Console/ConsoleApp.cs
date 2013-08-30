using System;
using System.Threading;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Host;

namespace NzbDrone.Console
{
    public static class ConsoleApp
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            try
            {
                Bootstrap.Start(new StartupArguments(args), new ConsoleAlerts());
            }
            catch (TerminateApplicationException)
            {
            }
            catch (Exception e)
            {
                Logger.FatalException(e.Message, e);
                System.Console.ReadLine();
            }

            while (true)
            {
                Thread.Sleep(10 * 60);
            }
        }
    }
}