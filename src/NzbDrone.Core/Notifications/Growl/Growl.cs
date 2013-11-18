using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Growl
{
    public class Growl : NotificationBase<GrowlSettings>
    {
        private readonly IGrowlService _growlProvider;

        public Growl(IGrowlService growlProvider)
        {
            _growlProvider = growlProvider;
        }

        public override string Link
        {
            get { return "http://growl.info/"; }
        }

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _growlProvider.SendNotification(title, message, "GRAB", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _growlProvider.SendNotification(title, message.Message, "DOWNLOAD", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
