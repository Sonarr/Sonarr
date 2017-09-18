using System.Collections.Generic;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http.Dispatchers;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        private UpdateContainerBuilder(IStartupContext startupContext, List<string> assemblies)
            : base(startupContext, assemblies)
        {
            Container.Register<IHttpDispatcher, FallbackHttpDispatcher>();
        }

        public static IContainer Build(IStartupContext startupContext)
        {
            var assemblies = new List<string>
                             {
                                 "Sonarr.Update"
                             };

            return new UpdateContainerBuilder(startupContext, assemblies).Container;
        }
    }
}
