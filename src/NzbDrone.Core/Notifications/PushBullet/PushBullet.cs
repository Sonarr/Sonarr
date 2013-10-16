using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBullet : NotificationBase<PushBulletSettings>
    {
        private readonly IPushBulletProxy _pushBulletProxy;

        public PushBullet(IPushBulletProxy pushBulletProxy)
        {
            _pushBulletProxy = pushBulletProxy;
        }

        public override string Link
        {
            get { return "https://www.pushbullet.com/"; }
        }

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _pushBulletProxy.SendNotification(title, message, Settings.ApiKey, Settings.DeviceId);
        }

        public override void OnDownload(string message, Series series)
        {
            const string title = "Episode Downloaded";

            _pushBulletProxy.SendNotification(title, message, Settings.ApiKey, Settings.DeviceId);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
