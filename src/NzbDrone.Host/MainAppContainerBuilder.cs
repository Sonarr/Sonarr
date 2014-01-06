using System;
using System.Collections.Generic;
using System.Linq;
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
            var assemblies = new List<String>
                             {
                                 "NzbDrone.Host",
                                 "NzbDrone.Common",
                                 "NzbDrone.Core",
                                 "NzbDrone.Api",
                                 "NzbDrone.SignalR"
                             };

            if (OsInfo.IsWindows)
            {
                assemblies.Add("NzbDrone.Windows");
            }

            else
            {
                assemblies.Add("NzbDrone.Mono");
            }

            return new MainAppContainerBuilder(args, assemblies.ToArray()).Container;
        }

        private MainAppContainerBuilder(StartupContext args, string[] assemblies)
            : base(args, assemblies)
        {
            AutoRegisterImplementations<NzbDronePersistentConnection>();

            Container.Register(typeof(IBasicRepository<NamingConfig>), typeof(BasicRepository<NamingConfig>));

            Container.Register<INancyBootstrapper, NancyBootstrapper>();
        }
    }
}