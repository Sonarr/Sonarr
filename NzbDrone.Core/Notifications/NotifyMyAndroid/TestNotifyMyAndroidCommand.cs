using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public class TestNotifyMyAndroidCommand : Command
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
