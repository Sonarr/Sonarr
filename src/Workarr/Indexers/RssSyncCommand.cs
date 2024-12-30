using Workarr.Messaging.Commands;

namespace Workarr.Indexers
{
    public class RssSyncCommand : Command
    {
        public override bool SendUpdatesToClient => true;
        public override bool IsLongRunning => true;
    }
}
