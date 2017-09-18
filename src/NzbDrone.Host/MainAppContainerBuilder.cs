using System.Collections.Generic;
using Nancy.Bootstrapper;
using Sonarr.Http;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.SignalR;

namespace NzbDrone.Host
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        public static IContainer BuildContainer(StartupContext args)
        {
            var assemblies = new List<string>
                             {
                                 "Sonarr.Host",
                                 "Sonarr.Core",
                                 "Sonarr.Api",
                                 "Sonarr.SignalR",
                                 "Sonarr.Api.V3",
                                 "Sonarr.Http"
                             };

            return new MainAppContainerBuilder(args, assemblies).Container;
        }

        private MainAppContainerBuilder(StartupContext args, List<string> assemblies)
            : base(args, assemblies)
        {
            AutoRegisterImplementations<NzbDronePersistentConnection>();

            Container.Register<INancyBootstrapper, SonarrBootstrapper>();
            Container.Register<IHttpDispatcher, FallbackHttpDispatcher>();
        }
    }
}
