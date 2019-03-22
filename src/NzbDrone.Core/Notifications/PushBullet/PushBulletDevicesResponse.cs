using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBulletDevicesResponse
    {
        public List<PushBulletDevice> Devices { get; set; }
    }
}
