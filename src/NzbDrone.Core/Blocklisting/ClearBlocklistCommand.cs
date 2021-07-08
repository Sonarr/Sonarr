using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Blocklisting
{
    public class ClearBlocklistCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
