using NLog;
using Nancy.Bootstrapper;
using NzbDrone.Api;
using NzbDrone.Api.SignalR;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        private static readonly Logger Logger = LogManager.GetLogger("ContainerBuilderBase");

        public static IContainer BuildContainer()
        {
            return new MainAppContainerBuilder().Container;
        }


        private MainAppContainerBuilder()
            : base("NzbDrone", "NzbDrone.Common", "NzbDrone.Core", "NzbDrone.Api")
        {
            AutoRegisterImplementations<NzbDronePersistentConnection>();

            Container.Register(typeof(IBasicRepository<RootFolder>), typeof(BasicRepository<RootFolder>));
            Container.Register(typeof(IBasicRepository<NamingConfig>), typeof(BasicRepository<NamingConfig>));

            Container.Register<INancyBootstrapper, NancyBootstrapper>();
        
        }


    }
}