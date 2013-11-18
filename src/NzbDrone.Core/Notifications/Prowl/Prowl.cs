using NzbDrone.Core.Tv;
using Prowlin;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class Prowl : NotificationBase<ProwlSettings>
    {
        private readonly IProwlService _prowlProvider;

        public Prowl(IProwlService prowlProvider)
        {
            _prowlProvider = prowlProvider;
        }

        public override string Link
        {
            get { return "http://www.prowlapp.com/"; }
        }

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _prowlProvider.SendNotification(title, message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _prowlProvider.SendNotification(title, message.Message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
