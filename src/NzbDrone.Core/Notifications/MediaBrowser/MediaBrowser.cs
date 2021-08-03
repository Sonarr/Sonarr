using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowser : NotificationBase<MediaBrowserSettings>
    {
        private readonly IMediaBrowserService _mediaBrowserService;

        public MediaBrowser(IMediaBrowserService mediaBrowserService)
        {
            _mediaBrowserService = mediaBrowserService;
        }

        public override string Link => "https://emby.media/";
        public override string Name => "Emby";

        public override void OnGrab(GrabMessage grabMessage)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, EPISODE_GRABBED_TITLE_BRANDED, grabMessage.Message);
            }
        }

        public override void OnDownload(DownloadMessage message)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, EPISODE_DOWNLOADED_TITLE_BRANDED, message.Message);
            }

            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.Update(Settings, message.Series, "Created");
            }
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.Update(Settings, series, "Modified");
            }
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, EPISODE_DELETED_TITLE_BRANDED, deleteMessage.Message);
            }

            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.Update(Settings, deleteMessage.Series, "Deleted");
            }
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, SERIES_DELETED_TITLE_BRANDED, deleteMessage.Message);
            }

            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.Update(Settings, deleteMessage.Series, "Deleted");
            }
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, HEALTH_ISSUE_TITLE_BRANDED, message.Message);
            }
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, APPLICATION_UPDATE_TITLE_BRANDED, updateMessage.Message);
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_mediaBrowserService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
