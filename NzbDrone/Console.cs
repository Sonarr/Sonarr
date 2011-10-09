using System;
using System.Linq;
using System.Reflection;
using NLog;
using Ninject;
using NzbDrone.Providers;

namespace NzbDrone
{
    public static class Console
    {
        private static readonly StandardKernel Kernel = new StandardKernel();

        private static readonly Logger Logger = LogManager.GetLogger("Host.Main");

        private static void Main(string[] args)
        {
            try
            {
                System.Console.WriteLine("Starting NzbDrone Console. Version " + Assembly.GetExecutingAssembly().GetName().Version);

                Kernel.Bind<ConfigProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<ConsoleProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<DebuggerProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<EnviromentProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<IISProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<MonitoringProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<ProcessProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<ServiceProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<WebClientProvider>().ToSelf().InSingletonScope();

                Kernel.Bind<ApplicationMode>().ToConstant(GetApplicationMode(args));

                Kernel.Get<Router>().Route();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
                Logger.Fatal(e.ToString());
            }

            System.Console.WriteLine("Press enter to exit.");
            System.Console.ReadLine();
        }

        public static ApplicationMode GetApplicationMode(string[] args)
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