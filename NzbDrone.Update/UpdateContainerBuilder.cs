using NzbDrone.Common;
using NzbDrone.Common.Composition;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        public UpdateContainerBuilder()
            : base("NzbDrone.Update", "NzbDrone.Common")
        {

        }

        public static IContainer Build()
        {
            return new UpdateContainerBuilder().Container;
        }
    }
}