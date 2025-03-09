using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;

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
            _proxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage?.Message, GetPosterUrl(grabMessage.Series), GetLinks(grabMessage.Series), Settings);
        }

        public override void OnDownload(DownloadMessage downloadMessage)
        {
            _proxy.SendNotification(EPISODE_DOWNLOADED_TITLE, downloadMessage.Message, GetPosterUrl(downloadMessage.Series), GetLinks(downloadMessage.Series), Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            _proxy.SendNotification(IMPORT_COMPLETE_TITLE, message.Message, GetPosterUrl(message.Series), GetLinks(message.Series), Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(EPISODE_DELETED_TITLE, deleteMessage.Message, GetPosterUrl(deleteMessage.Series), GetLinks(deleteMessage.Series), Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage seriesAddMessage)
        {
            _proxy.SendNotification(SERIES_ADDED_TITLE, $"{seriesAddMessage.Series.Title} added to library", GetPosterUrl(seriesAddMessage.Series), GetLinks(seriesAddMessage.Series), Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(SERIES_DELETED_TITLE, deleteMessage.Message, GetPosterUrl(deleteMessage.Series), GetLinks(deleteMessage.Series), Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE_BRANDED, healthCheck.Message, null, [], Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            _proxy.SendNotification(HEALTH_RESTORED_TITLE_BRANDED, $"The following issue is now resolved: {previousCheck.Message}", null, [], Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE_BRANDED, updateMessage.Message, null, [], Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage manualInteractionRequiredMessage)
        {
            _proxy.SendNotification(MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED, manualInteractionRequiredMessage.Message, null, [], Settings);
        }

        private string GetPosterUrl(Series series)
        {
            return series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl;
        }

        private List<NotificationMetadataLink> GetLinks(Series series)
        {
            return NotificationMetadataLinkGenerator.GenerateLinks(series, Settings.MetadataLinks);
        }
    }
}
