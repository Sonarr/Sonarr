using Nancy.Bootstrapper;
using NzbDrone.Api;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Organizer;
using NzbDrone.SignalR;

namespace NzbDrone.Host
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        public static IContainer BuildContainer(StartupContext args)
        {
            return new MainAppContainerBuilder(args).Container;
        }

        private MainAppContainerBuilder(StartupContext args)
            : base(args, "NzbDrone.Host", "NzbDrone.Common", "NzbDrone.Core", "NzbDrone.Api", "NzbDrone.SignalR")
        {

            AutoRegisterImplementations<NzbDronePersistentConnection>();

            Container.Register(typeof(IBasicRepository<NamingConfig>), typeof(BasicRepository<NamingConfig>));

            Container.Register<INancyBootstrapper, NancyBootstrapper>();
        }
    }
}