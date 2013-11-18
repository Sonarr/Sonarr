using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class Pushover : NotificationBase<PushoverSettings>
    {
        private readonly IPushoverProxy _pushoverProxy;

        public Pushover(IPushoverProxy pushoverProxy)
        {
            _pushoverProxy = pushoverProxy;
        }

        public override string Link
        {
            get { return "https://pushover.net/"; }
        }

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _pushoverProxy.SendNotification(title, message, Settings.UserKey, (PushoverPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _pushoverProxy.SendNotification(title, message.Message, Settings.UserKey, (PushoverPriority)Settings.Priority);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
