using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;

namespace NzbDrone.Core.Notifications.MailGun
{
    public class MailGun : NotificationBase<MailGunSettings>
    {
        private readonly IMailGunProxy _proxy;
        private readonly Logger _logger;
        
        public MailGun(IMailGunProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public override string Name => "MailGun";
        public override string Link => "https://mailgun.com";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage.Message, Settings);
        }


        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr, though MailGun.";

                _proxy.SendNotification(title, body, Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message though MailGun.");
                failures.Add(new ValidationFailure("", "Unable to send test message though MailGun."));
            }
            
            return new ValidationResult(failures);
        }
    }
}
