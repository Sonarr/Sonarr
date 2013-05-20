using NzbDrone.Common;
using NzbDrone.Common.Composition;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        private UpdateContainerBuilder()
            : base("NzbDrone.Update", "NzbDrone.Common")
        {

        }

        public static IContainer Build()
        {
            return new UpdateContainerBuilder().Container;
        }
    }
}