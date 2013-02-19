using System;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Smtp: ExternalNotificationBase
    {
        private readonly SmtpProvider _smtpProvider;

        public Smtp(ConfigProvider configProvider, SmtpProvider smtpProvider)
            : base(configProvider)
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

            if (_configProvider.SmtpNotifyOnGrab)
            {
                _logger.Trace("Sending SMTP Notification");
                _smtpProvider.SendEmail(subject, body);
            }
        }

        public override void OnDownload(string message, Series series)
        {
            const string subject = "NzbDrone [TV] - Downloaded";
            var body = String.Format("{0} Downloaded and sorted.", message);

            if (_configProvider.SmtpNotifyOnDownload)
            {
                _logger.Trace("Sending SMTP Notification");
                _smtpProvider.SendEmail(subject, body);
            }
        }

        public override void OnRename(string message, Series series)
        {
            
        }

        public override void AfterRename(string message, Series series)
        {

        }
    }
}
