using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ninject;
using NzbDrone.Model;
using NzbDrone.Providers;

namespace NzbDrone
{
    public static class CentralDispatch
    {
        private static StandardKernel _kernel;

        static CentralDispatch()
        {
            _kernel = new StandardKernel();
        }

        public static ApplicationMode ApplicationMode { get; set; }

        public static StandardKernel Kernel
        {
            get
            {
                return _kernel;
            }
        }

        private static void BindKernel()
        {
            _kernel = new StandardKernel();
            _kernel.Bind<ConfigProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<ConsoleProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<DebuggerProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<EnviromentProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<IISProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<MonitoringProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<ProcessProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<ServiceProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<WebClientProvider>().ToSelf().InSingletonScope();
        }

        private static void InitilizeApp()
        {
            _kernel.Get<ConfigProvider>().ConfigureNlog();
            _kernel.Get<ConfigProvider>().CreateDefaultConfigFile();
            Logger.Info("Starting NZBDrone. Start-up Path:'{0}'", _kernel.Get<EnviromentProvider>().ApplicationPath);
            Thread.CurrentThread.Name = "Host";
        }
    }
}
