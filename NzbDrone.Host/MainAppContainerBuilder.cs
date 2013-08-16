using Nancy.Bootstrapper;
using NzbDrone.Api;
using NzbDrone.Api.SignalR;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Host
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        public static IContainer BuildContainer(StartupArguments args)
        {
            return new MainAppContainerBuilder(args).Container;
        }

        private MainAppContainerBuilder(StartupArguments args)
            : base(args, "NzbDrone.Host", "NzbDrone.Common", "NzbDrone.Core", "NzbDrone.Api")
        {

            AutoRegisterImplementations<NzbDronePersistentConnection>();

            Container.Register(typeof(IBasicRepository<RootFolder>), typeof(BasicRepository<RootFolder>));
            Container.Register(typeof(IBasicRepository<NamingConfig>), typeof(BasicRepository<NamingConfig>));

            Container.Register<INancyBootstrapper, NancyBootstrapper>();
        }
    }
}