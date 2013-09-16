using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class TestProwlCommand : Command
    {
        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
        public string ApiKey { get; set; }
        public int Priority { get; set; }
    }
}
