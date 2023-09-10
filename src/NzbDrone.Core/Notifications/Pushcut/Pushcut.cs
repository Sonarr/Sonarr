using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public class Pushcut : NotificationBase<PushcutSettings>
    {
        private readonly IPushcutProxy _proxy;

        public Pushcut(IPushcutProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Pushcut";

        public override string Link => "https://www.pushcut.io";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage?.Message, Settings);
        }

        public override void OnDownload(DownloadMessage downloadMessage)
        {
            _proxy.SendNotification(EPISODE_DOWNLOADED_TITLE, downloadMessage.Message, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(EPISODE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage seriesAddMessage)
        {
            _proxy.SendNotification(SERIES_ADDED_TITLE, $"{seriesAddMessage.Series.Title} added to library", Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(SERIES_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE_BRANDED, healthCheck.Message, Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            _proxy.SendNotification(HEALTH_RESTORED_TITLE_BRANDED, $"The following issue is now resolved: {previousCheck.Message}", Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE_BRANDED, updateMessage.Message, Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage manualInteractionRequiredMessage)
        {
            _proxy.SendNotification(MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED, manualInteractionRequiredMessage.Message, Settings);
        }
    }
}
