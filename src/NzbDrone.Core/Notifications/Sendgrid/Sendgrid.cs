using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Sendgrid
{
    public class Sendgrid: NotificationBase<SendgridSettings>
    {
        private readonly ISendGridService _sendGridService;

        public override string Name => "SendGrid";
        
        public Sendgrid(ISendGridService sendGridService)
        {
            _sendGridService = sendGridService;
        }

        public override string Link => null;

        public override void OnGrab(GrabMessage grabMessage)
        {
            var body = $"{grabMessage.Message} sent to queue.";
            _sendGridService.Send(Settings, EPISODE_GRABBED_TITLE, body);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var body = $"{message.Message} Downloaded and sorted.";
            _sendGridService.Send(Settings, EPISODE_GRABBED_TITLE, body);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_sendGridService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}