
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Tv;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class Webhook : NotificationBase<WebhookSettings>
    {
        private readonly IWebhookService _service;

        public Webhook(IWebhookService service)
        {
            _service = service;
        }

        public override string Link => "https://github.com/Sonarr/Sonarr/wiki/Webhook";

        public override void OnGrab(GrabMessage message)
        {
            _service.OnGrab(message.Series, message.Episode, message.Quality, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _service.OnDownload(message.Series, message.EpisodeFile, Settings);
        }

        public override void OnRename(Series series)
        {
            _service.OnRename(series, Settings);
        }

        public override string Name => "Webhook";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_service.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
