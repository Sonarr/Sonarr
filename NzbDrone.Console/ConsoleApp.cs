using System;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Console
{
    public static class ConsoleApp
    {
        public static void Main(string[] args)
        {
            try
            {
                Host.Bootstrap.Start(new StartupArguments(args));
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }

            System.Console.WriteLine("Press enter to exit...");
            System.Console.ReadLine();
        }
    }
}