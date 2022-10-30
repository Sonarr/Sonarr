using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;

namespace NzbDrone.Core.Notifications.Mailgun
{
    public class MailGun : NotificationBase<MailgunSettings>
    {
        private readonly IMailgunProxy _proxy;
        private readonly Logger _logger;

        public MailGun(IMailgunProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public override string Name => "Mailgun";
        public override string Link => "https://mailgun.com";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage downloadMessage)
        {
            _proxy.SendNotification(EPISODE_DOWNLOADED_TITLE, downloadMessage.Message, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var body = $"{deleteMessage.Message} deleted.";

            _proxy.SendNotification(EPISODE_DELETED_TITLE, body, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var body = $"{deleteMessage.Message}";

            _proxy.SendNotification(SERIES_DELETED_TITLE, body, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheckMessage)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE, healthCheckMessage.Message, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE, updateMessage.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr, though Mailgun.";

                _proxy.SendNotification(title, body, Settings);
                _logger.Info("Successfully sent email though Mailgun.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message though Mailgun.");
                failures.Add(new ValidationFailure("", "Unable to send test message though Mailgun."));
            }

            return new ValidationResult(failures);
        }
    }
}
