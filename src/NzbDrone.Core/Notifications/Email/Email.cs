using System;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Email
{
    public class Email : NotificationBase<EmailSettings>
    {
        private readonly IEmailService _smtpProvider;

        public Email(IEmailService smtpProvider)
        {
            _smtpProvider = smtpProvider;
        }

        public override string Link
        {
            get { return null; }
        }

        public override void OnGrab(string message)
        {
            const string subject = "NzbDrone [TV] - Grabbed";
            var body = String.Format("{0} sent to SABnzbd queue.", message);

            _smtpProvider.SendEmail(Settings, subject, body);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string subject = "NzbDrone [TV] - Downloaded";
            var body = String.Format("{0} Downloaded and sorted.", message.Message);

            _smtpProvider.SendEmail(Settings, subject, body);
        }

        public override void AfterRename(Series series)
        {
        }
    }
}
