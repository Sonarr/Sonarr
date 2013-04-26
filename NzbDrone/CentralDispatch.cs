using Autofac;
using NLog;
using NzbDrone.Common;
using NzbDrone.Providers;

namespace NzbDrone
{
    public static class CentralDispatch
    {
        private static IContainer _container;
        private static readonly Logger Logger = LogManager.GetLogger("Host.CentralDispatch");

        static CentralDispatch()
        {
            var builder = new ContainerBuilder();
            BindKernel(builder);
            _container = builder.Build();
            InitilizeApp();
        }

        public static IContainer Container
        {
            get
            {
                return _container;
            }
        }

        private static void BindKernel(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(DiskProvider).Assembly).SingleInstance();
            builder.RegisterType<Router>();

            builder.RegisterType<ApplicationServer>().SingleInstance();
            builder.RegisterType<ConfigFileProvider>().SingleInstance();
            builder.RegisterType<ConsoleProvider>().SingleInstance();
            builder.RegisterType<DebuggerProvider>().SingleInstance();
            builder.RegisterType<EnvironmentProvider>().SingleInstance();
            builder.RegisterType<IISProvider>().SingleInstance();
            builder.RegisterType<MonitoringProvider>().SingleInstance();
            builder.RegisterType<ProcessProvider>().SingleInstance();
            builder.RegisterType<ServiceProvider>().SingleInstance();
            builder.RegisterType<HttpProvider>().SingleInstance();
        }

        private static void InitilizeApp()
        {
            var environmentProvider = _container.Resolve<EnvironmentProvider>();
            
            ReportingService.RestProvider = _container.Resolve<RestProvider>();

            LogConfiguration.RegisterRollingFileLogger(environmentProvider.GetLogFileName(), LogLevel.Info);
            LogConfiguration.RegisterConsoleLogger(LogLevel.Debug);
            LogConfiguration.RegisterUdpLogger();
            LogConfiguration.Reload();
            Logger.Info("Start-up Path:'{0}'", environmentProvider.ApplicationPath);
        }
    }
}
