using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Blacklisting
{
    public class ClearBlacklistCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}