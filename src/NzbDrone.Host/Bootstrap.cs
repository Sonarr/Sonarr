using System.Reflection;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Security;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Host
{
    public class Bootstrap
    {
        public IContainer Container { get; private set; }

        public Bootstrap(StartupArguments args, IUserAlert userAlert)
        {
            var logger = NzbDroneLogger.GetLogger();

            GlobalExceptionHandlers.Register();
            IgnoreCertErrorPolicy.Register();

            logger.Info("Starting NzbDrone Console. Version {0}", Assembly.GetExecutingAssembly().GetName().Version);


            if (!PlatformValidation.IsValidate(userAlert))
            {
                throw new TerminateApplicationException("Missing system requirements");
            }

            Container = MainAppContainerBuilder.BuildContainer(args);


        }

        public void Start()
        {
            DbFactory.RegisterDatabase(Container);
            Container.Resolve<Router>().Route();
        }


        public void EnsureSingleInstance()
        {
            Container.Resolve<ISingleInstancePolicy>().EnforceSingleInstance();
        }

    }
}