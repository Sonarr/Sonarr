using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Providers;

namespace NzbDrone
{
    public static class CentralDispatch
    {
        private static StandardKernel _kernel;
        private static readonly Logger Logger = LogManager.GetLogger("Host.CentralDispatch");

        static CentralDispatch()
        {
            _kernel = new StandardKernel();
            BindKernel();
            InitilizeApp();
        }

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
            _kernel.Bind<ApplicationServer>().ToSelf().InSingletonScope();
            _kernel.Bind<ConfigFileProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<ConsoleProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<DebuggerProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<EnviromentProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<IISProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<MonitoringProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<ProcessProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<ServiceProvider>().ToSelf().InSingletonScope();
            _kernel.Bind<HttpProvider>().ToSelf().InSingletonScope();

        }

        private static void InitilizeApp()
        {
            var enviromentProvider = _kernel.Get<EnviromentProvider>();

            LogConfiguration.RegisterRollingFileLogger(enviromentProvider.GetLogFileName(), LogLevel.Info);
            LogConfiguration.RegisterConsoleLogger(LogLevel.Debug);
            LogConfiguration.RegisterUdpLogger();
            LogConfiguration.RegisterRemote();
            LogConfiguration.Reload();
            Logger.Info("Start-up Path:'{0}'", enviromentProvider.ApplicationPath);
        }
    }
}
