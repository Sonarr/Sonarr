using System.Reflection;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Security;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Host
{
    public static class Bootstrap
    {
        public static IContainer Start(StartupArguments args, IUserAlert userAlert)
        {
            var logger = LogManager.GetLogger("AppMain");

            GlobalExceptionHandlers.Register();
            IgnoreCertErrorPolicy.Register();

            logger.Info("Starting NzbDrone Console. Version {0}", Assembly.GetExecutingAssembly().GetName().Version);


            if (!PlatformValidation.IsValidate(userAlert))
            {
                throw new TerminateApplicationException();
            }

            var container = MainAppContainerBuilder.BuildContainer(args);

            DbFactory.RegisterDatabase(container);
            container.Resolve<Router>().Route();

            return container;
        }
    }
}