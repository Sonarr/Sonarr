using System.Collections.Generic;
using Nancy.Bootstrapper;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.SignalR;
using Sonarr.Http;

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
                                 "Sonarr.SignalR",
                                 "Sonarr.Api.V3",
                                 "Sonarr.Http"
                             };

            return new MainAppContainerBuilder(args, assemblies).Container;
        }

        private MainAppContainerBuilder(StartupContext args, List<string> assemblies)
            : base(args, assemblies)
        {
            AutoRegisterImplementations<MessageHub>();

            Container.Register<INancyBootstrapper, SonarrBootstrapper>();

            if (OsInfo.IsWindows)
            {
                Container.Register<INzbDroneServiceFactory, NzbDroneServiceFactory>();
            }
            else
            {
                Container.Register<INzbDroneServiceFactory, DummyNzbDroneServiceFactory>();
            }
        }
    }
}
