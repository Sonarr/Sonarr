using System.Collections.Generic;

using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Ntfy
{
    public class Ntfy : NotificationBase<NtfySettings>
    {
        private readonly INtfyProxy _proxy;

        public Ntfy(INtfyProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "ntfy.sh";

        public override string Link => "https://ntfy.sh/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(MOVIE_GRABBED_TITLE_BRANDED, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendNotification(MOVIE_DOWNLOADED_TITLE_BRANDED, message.Message, Settings);
        }

        public override void OnMovieAdded(Movie movie)
        {
            _proxy.SendNotification(MOVIE_ADDED_TITLE_BRANDED, $"{movie.Title} added to library", Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(MOVIE_FILE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(MOVIE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE_BRANDED, healthCheck.Message, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE_BRANDED, updateMessage.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
