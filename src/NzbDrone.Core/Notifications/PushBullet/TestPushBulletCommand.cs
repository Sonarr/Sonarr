using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class TestPushBulletCommand : Command
    {

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
        public string ApiKey { get; set; }
        public string DeviceId { get; set; }
    }
}
