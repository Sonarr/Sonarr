using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.Notifications.Email
{
    public class Email : NotificationBase<EmailSettings>
    {
        private readonly ICertificateValidationService _certificateValidationService;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public override string Name => _localizationService.GetLocalizedString("NotificationsEmailSettingsName");

        public Email(ICertificateValidationService certificateValidationService, ILocalizationService localizationService, Logger logger)
        {
            _certificateValidationService = certificateValidationService;
            _localizationService = localizationService;
            _logger = logger;
        }

        public override string Link => null;

        public override void OnGrab(GrabMessage grabMessage)
        {
            var body = $"{grabMessage.Message} sent to queue.";

            SendEmail(Settings, EPISODE_GRABBED_TITLE_BRANDED, body);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var body = $"{message.Message} Downloaded and sorted.";

            SendEmail(Settings, EPISODE_DOWNLOADED_TITLE_BRANDED, body);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var body = $"{deleteMessage.Message} deleted.";

            SendEmail(Settings, EPISODE_DELETED_TITLE_BRANDED, body);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var body = $"{message.Message}";

            SendEmail(Settings, SERIES_ADDED_TITLE_BRANDED, body);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var body = $"{deleteMessage.Message}";

            SendEmail(Settings, SERIES_DELETED_TITLE_BRANDED, body);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            SendEmail(Settings, HEALTH_ISSUE_TITLE_BRANDED, message.Message);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousMessage)
        {
            SendEmail(Settings, HEALTH_RESTORED_TITLE_BRANDED, $"The following issue is now resolved: {previousMessage.Message}");
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var body = $"{updateMessage.Message}";

            SendEmail(Settings, APPLICATION_UPDATE_TITLE_BRANDED, body);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var body = $"{message.Message} requires manual interaction.";

            SendEmail(Settings, MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED, body);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(Test(Settings));

            return new ValidationResult(failures);
        }

        private void SendEmail(EmailSettings settings, string subject, string body, bool htmlBody = false)
        {
            var email = new MimeMessage();

            email.From.Add(ParseAddress("From", settings.From));
            email.To.AddRange(settings.To.Select(x => ParseAddress("To", x)));
            email.Cc.AddRange(settings.Cc.Select(x => ParseAddress("CC", x)));
            email.Bcc.AddRange(settings.Bcc.Select(x => ParseAddress("BCC", x)));

            email.Subject = subject;
            email.Body = new TextPart(htmlBody ? "html" : "plain")
            {
                Text = body
            };

            _logger.Debug("Sending email Subject: {0}", subject);

            try
            {
                Send(email, settings);
                _logger.Debug("Email sent. Subject: {0}", subject);
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email. Subject: {0}", email.Subject);
                _logger.Debug(ex, ex.Message);
                throw;
            }

            _logger.Debug("Finished sending email. Subject: {0}", subject);
        }

        private void Send(MimeMessage email, EmailSettings settings)
        {
            using var client = new SmtpClient();
            client.Timeout = 10000;

            var useEncyption = (EmailEncryptionType)settings.UseEncryption;

            var serverOption = useEncyption switch
            {
                EmailEncryptionType.Always => settings.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls,
                EmailEncryptionType.Never => SecureSocketOptions.None,
                _ => SecureSocketOptions.Auto
            };

            client.ServerCertificateValidationCallback = _certificateValidationService.ShouldByPassValidationError;

            _logger.Debug("Connecting to mail server");

            client.Connect(settings.Server, settings.Port, serverOption);

            if (!string.IsNullOrWhiteSpace(settings.Username))
            {
                _logger.Debug("Authenticating to mail server");

                client.Authenticate(settings.Username, settings.Password);
            }

            _logger.Debug("Sending to mail server");

            client.Send(email);

            _logger.Debug("Sent to mail server, disconnecting");

            client.Disconnect(true);

            _logger.Debug("Disconnecting from mail server");
        }

        public ValidationFailure Test(EmailSettings settings)
        {
            const string body = "Success! You have properly configured your email notification settings";

            try
            {
                SendEmail(settings, "Sonarr - Test Notification", body);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test email");
                return new ValidationFailure("Server", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private MailboxAddress ParseAddress(string type, string address)
        {
            try
            {
                return MailboxAddress.Parse(address);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0} email address '{1}' invalid", type, address);
                throw;
            }
        }
    }
}
