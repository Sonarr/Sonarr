using System.Reflection;
using Autofac;
using NLog;
using NzbDrone.Api;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;

namespace NzbDrone
{
    public static class NzbDroneBootstrapper
    {
        private static readonly IContainer container;
        private static readonly Logger logger = LogManager.GetLogger("NzbDroneBootstrapper");

        static NzbDroneBootstrapper()
        {
            var builder = new ContainerBuilder();
            BindKernel(builder);
            container = builder.Build();
            InitializeApp();
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
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly());
        }

        private static void InitializeApp()
        {
            var environmentProvider = container.Resolve<EnvironmentProvider>();

            ReportingService.RestProvider = container.Resolve<RestProvider>();

            logger.Info("Start-up Path:'{0}'", environmentProvider.WorkingDirectory);
        }
    }
}
