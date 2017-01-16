using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Configuration
{
    public class ResetApiKeyCommand : Command
    {
        public override bool SendUpdatesToClient => true;
    }
}
