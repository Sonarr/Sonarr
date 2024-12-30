using Workarr.Messaging.Commands;

namespace Workarr.Blocklisting
{
    public class ClearBlocklistCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
