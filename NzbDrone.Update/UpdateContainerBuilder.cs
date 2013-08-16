using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        private UpdateContainerBuilder(IStartupArguments startupArguments)
            : base(startupArguments, "NzbDrone.Update", "NzbDrone.Common")
        {

        }

        public static IContainer Build(IStartupArguments startupArguments)
        {
            return new UpdateContainerBuilder(startupArguments).Container;
        }
    }
}