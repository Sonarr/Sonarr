using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Simplepush
{
    public class Simplepush : NotificationBase<SimplepushSettings>
    {
        private readonly ISimplepushProxy _proxy;

        public Simplepush(ISimplepushProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Simplepush";
        public override string Link => "https://simplepush.io/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendNotification(EPISODE_DOWNLOADED_TITLE, message.Message, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(EPISODE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(SERIES_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE, updateMessage.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
