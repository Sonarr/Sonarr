using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Lifecycle.Commands
{
    public class ShutdownCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
    }
}
