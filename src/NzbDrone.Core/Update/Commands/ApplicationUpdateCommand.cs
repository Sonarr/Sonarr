using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Update.Commands
{
    public class ApplicationUpdateCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public override string CompletionMessage
        {
            get
            {
                return "Restarting Sonarr to apply updates";
            }
        }
    }
}
