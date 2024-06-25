using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class Prowl : NotificationBase<ProwlSettings>
    {
        private readonly IProwlProxy _prowlProxy;

        public Prowl(IProwlProxy prowlProxy)
        {
            _prowlProxy = prowlProxy;
        }

        public override string Link => "https://www.prowlapp.com/";
        public override string Name => "Prowl";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _prowlProxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _prowlProxy.SendNotification(EPISODE_DOWNLOADED_TITLE, message.Message, Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            _prowlProxy.SendNotification(IMPORT_COMPLETE_TITLE, message.Message, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _prowlProxy.SendNotification(EPISODE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            _prowlProxy.SendNotification(SERIES_ADDED_TITLE, message.Message, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _prowlProxy.SendNotification(SERIES_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            _prowlProxy.SendNotification(HEALTH_ISSUE_TITLE, message.Message, Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousMessage)
        {
            _prowlProxy.SendNotification(HEALTH_RESTORED_TITLE, $"The following issue is now resolved: {previousMessage.Message}", Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _prowlProxy.SendNotification(APPLICATION_UPDATE_TITLE, updateMessage.Message, Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            _prowlProxy.SendNotification(MANUAL_INTERACTION_REQUIRED_TITLE, message.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_prowlProxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
