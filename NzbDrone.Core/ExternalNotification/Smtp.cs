using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ExternalNotification
{
    public class Smtp : ExternalNotificationBase
    {
        private readonly SmtpProvider _smtpProvider;

        public Smtp(IExternalNotificationRepository repository, SmtpProvider smtpProvider, Logger logger)
            : base(repository, logger)
        {
            _smtpProvider = smtpProvider;
        }

        public override string Name
        {
            get { return "SMTP"; }
        }

        protected override void OnGrab(string message)
        {
            const string subject = "NzbDrone [TV] - Grabbed";
            var body = String.Format("{0} sent to SABnzbd queue.", message);
            _smtpProvider.SendEmail(subject, body);
        }

        protected override void OnDownload(string message, Series series)
        {
            const string subject = "NzbDrone [TV] - Downloaded";
            var body = String.Format("{0} Downloaded and sorted.", message);

            _smtpProvider.SendEmail(subject, body);
        }
    }
}
