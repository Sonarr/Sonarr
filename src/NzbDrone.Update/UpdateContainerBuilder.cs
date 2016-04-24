using System;
using System.Collections.Generic;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http.Dispatchers;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        private UpdateContainerBuilder(IStartupContext startupContext, string[] assemblies)
            : base(startupContext, assemblies)
        {
            Container.Register<IHttpDispatcher, FallbackHttpDispatcher>();
        }

        public static IContainer Build(IStartupContext startupContext)
        {
            var assemblies = new List<string>
                             {
                                 "NzbDrone.Update",
                                 "NzbDrone.Common"
                             };

            if (OsInfo.IsWindows)
            {
                assemblies.Add("NzbDrone.Windows");
            }

            else
            {
                assemblies.Add("NzbDrone.Mono");
            }

            return new UpdateContainerBuilder(startupContext, assemblies.ToArray()).Container;
        }
    }
}
 