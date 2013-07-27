using NzbDrone.Core.Notifications.Prowl;
using NzbDrone.Core.Tv;
using Prowlin;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class Pushover : NotificationBase<PushoverSettings>
    {
        private readonly IPushoverService _pushoverService;

        public Pushover(IPushoverService pushoverService)
        {
            _pushoverService = pushoverService;
        }

        public override string Name
        {
            get { return "Pushover"; }
        }

        public override string ImplementationName
        {
            get { return "Pushover"; }
        }

        public override string Link
        {
            get { return "https://pushover.net/"; }
        }

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _pushoverService.SendNotification(title, message, Settings.UserKey, (PushoverPriority)Settings.Priority);
        }

        public override void OnDownload(string message, Series series)
        {
            const string title = "Episode Downloaded";

            _pushoverService.SendNotification(title, message, Settings.UserKey, (PushoverPriority)Settings.Priority);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
