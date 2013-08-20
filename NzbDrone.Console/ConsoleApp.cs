using System;
using System.Threading;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Host;

namespace NzbDrone.Console
{
    public static class ConsoleApp
    {
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
                System.Console.WriteLine(e.ToString());
                System.Console.ReadLine();
            }

            while (true)
            {
                Thread.Sleep(10 * 60);
            }
        }
    }
}