using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        private UpdateContainerBuilder(IStartupContext startupContext)
            : base(startupContext, "NzbDrone.Update", "NzbDrone.Common")
        {

        }

        public static IContainer Build(IStartupContext startupContext)
        {
            return new UpdateContainerBuilder(startupContext).Container;
        }
    }
}