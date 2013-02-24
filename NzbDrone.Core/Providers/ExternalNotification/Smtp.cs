using System;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Smtp: ExternalNotificationBase
    {
        private readonly SmtpProvider _smtpProvider;

        public Smtp(IConfigService configService, SmtpProvider smtpProvider)
            : base(configService)
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

            if (_configService.SmtpNotifyOnGrab)
            {
                _logger.Trace("Sending SMTP Notification");
                _smtpProvider.SendEmail(subject, body);
            }
        }

        public override void OnDownload(string message, Series series)
        {
            const string subject = "NzbDrone [TV] - Downloaded";
            var body = String.Format("{0} Downloaded and sorted.", message);

            if (_configService.SmtpNotifyOnDownload)
            {
                _logger.Trace("Sending SMTP Notification");
                _smtpProvider.SendEmail(subject, body);
            }
        }



        public override void AfterRename(string message, Series series)
        {

        }
    }
}
