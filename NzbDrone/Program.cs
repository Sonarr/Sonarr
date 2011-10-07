using System;
using NLog;
using Ninject;
using NzbDrone.Providers;

namespace NzbDrone
{
    public static class Program
    {
        public static readonly StandardKernel Kernel = new StandardKernel();

        private static readonly Logger Logger = LogManager.GetLogger("Host.Main");

        private static void Main()
        {
            try
            {
                Kernel.Bind<ConfigProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<ConsoleProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<DebuggerProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<EnviromentProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<IISProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<MonitoringProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<ProcessProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<ServiceProvider>().ToSelf().InSingletonScope();
                Kernel.Bind<WebClientProvider>().ToSelf().InSingletonScope();

                Console.WriteLine("Starting Console.");
                Kernel.Get<MonitoringProvider>().Start();
                Kernel.Get<Application>().Start();
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Fatal(e.ToString());
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}