using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Indexers
{
    public class RssSyncCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
