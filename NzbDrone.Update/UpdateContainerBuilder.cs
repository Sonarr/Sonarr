using NzbDrone.Common;
using TinyIoC;

namespace NzbDrone.Update
{
    public class UpdateContainerBuilder : ContainerBuilderBase
    {
        public UpdateContainerBuilder()
            : base("NzbDrone.Update", "NzbDrone.Common")
        {

        }

        public static TinyIoCContainer Build()
        {
            return new UpdateContainerBuilder().Container;
        }
    }
}