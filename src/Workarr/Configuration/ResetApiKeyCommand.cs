using Workarr.Messaging.Commands;

namespace Workarr.Configuration
{
    public class ResetApiKeyCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
