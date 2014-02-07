using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Lifecycle.Commands
{
    public class RestartCommand : Command
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
