using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Ninject;
using NzbDrone.Model;

namespace NzbDrone
{
    public static class NzbDroneConsole
    {


        private static readonly Logger Logger = LogManager.GetLogger("Host.Main");

        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting NzbDrone Console. Version " + Assembly.GetExecutingAssembly().GetName().Version);

                CentralDispatch.ApplicationMode = GetApplicationMode(args);

                CentralDispatch.Kernel.Get<Router>().Route();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Fatal(e.ToString());
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        public static ApplicationMode GetApplicationMode(IEnumerable<string> args)
        {
            if (args == null) return ApplicationMode.Console;

            var cleanArgs = args.Where(c => c != null && !String.IsNullOrWhiteSpace(c)).ToList();
            if (cleanArgs.Count == 0) return ApplicationMode.Console;
            if (cleanArgs.Count != 1) return ApplicationMode.Help;

            var arg = cleanArgs.First().Trim('/', '\\', '-').ToLower();

            if (arg == "i") return ApplicationMode.InstallService;
            if (arg == "u") return ApplicationMode.UninstallService;

            return ApplicationMode.Help;
        }
    }
}