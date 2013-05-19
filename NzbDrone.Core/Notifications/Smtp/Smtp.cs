using System;
using NLog;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Smtp
{
    public class Smtp : NotificationWithSetting<SmtpSettings>
    {
        private readonly SmtpProvider _smtpProvider;

        public Smtp(SmtpProvider smtpProvider)
        {
            _smtpProvider = smtpProvider;
        }

        public override string Name
        {
            get { return "SMTP"; }
        }

        public override void OnGrab(string message)
        {
            const string subject = "NzbDrone [TV] - Grabbed";
            var body = String.Format("{0} sent to SABnzbd queue.", message);

            _smtpProvider.SendEmail(Settings, subject, body);
        }

        public override void OnDownload(string message, Series series)
        {
            const string subject = "NzbDrone [TV] - Downloaded";
            var body = String.Format("{0} Downloaded and sorted.", message);

            _smtpProvider.SendEmail(Settings, subject, body);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
