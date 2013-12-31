using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class TestPushoverCommand : Command
    {

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public string ApiKey { get; set; }
        public string UserKey { get; set; }
        public int Priority { get; set; }
    }
}
