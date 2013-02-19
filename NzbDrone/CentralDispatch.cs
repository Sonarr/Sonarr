using Autofac;
using NLog;
using NzbDrone.Api;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;

namespace NzbDrone
{
    public static class CentralDispatch
    {
        private static readonly IContainer container;
        private static readonly Logger logger = LogManager.GetLogger("Host.CentralDispatch");

        static CentralDispatch()
        {
            var builder = new ContainerBuilder();
            BindKernel(builder);
            container = builder.Build();
            InitilizeApp();
        }

        public static IContainer Container
        {
            get
            {
                return container;
            }
        }

        private static void BindKernel(ContainerBuilder builder)
        {
            builder.RegisterModule<LogInjectionModule>();

            builder.RegisterCommonServices();
            builder.RegisterApiServices();
            builder.RegisterAssemblyTypes("NzbDrone");
        }

        private static void InitilizeApp()
        {
            var environmentProvider = container.Resolve<EnvironmentProvider>();

            ReportingService.RestProvider = container.Resolve<RestProvider>();
            ReportingService.SetupExceptronDriver();

            LogConfiguration.RegisterRollingFileLogger(environmentProvider.GetLogFileName(), LogLevel.Info);
            LogConfiguration.RegisterConsoleLogger(LogLevel.Debug);
            LogConfiguration.RegisterUdpLogger();
            LogConfiguration.RegisterRemote();
            LogConfiguration.Reload();
            logger.Info("Start-up Path:'{0}'", environmentProvider.ApplicationPath);
        }
    }
}
