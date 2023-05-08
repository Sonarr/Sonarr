using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class Gotify : NotificationBase<GotifySettings>
    {
        private readonly IGotifyProxy _proxy;
        private readonly Logger _logger;

        public Gotify(IGotifyProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public override string Name => "Gotify";
        public override string Link => "https://gotify.net/";

        public override void OnGrab(GrabMessage message)
        {
            SendNotification(EPISODE_GRABBED_TITLE, message.Message, message.Series);
        }

        public override void OnDownload(DownloadMessage message)
        {
            SendNotification(EPISODE_DOWNLOADED_TITLE, message.Message, message.Series);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage message)
        {
            SendNotification(EPISODE_DELETED_TITLE, message.Message, message.Series);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            SendNotification(SERIES_ADDED_TITLE, message.Message, message.Series);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage message)
        {
            SendNotification(SERIES_DELETED_TITLE, message.Message, message.Series);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, null);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            SendNotification(HEALTH_RESTORED_TITLE, $"The following issue is now resolved: {previousCheck.Message}", null);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage message)
        {
            SendNotification(APPLICATION_UPDATE_TITLE, message.Message, null);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                var isMarkdown = false;
                const string title = "Test Notification";

                var sb = new StringBuilder();
                sb.AppendLine("This is a test message from Sonarr");

                if (Settings.IncludeSeriesPoster)
                {
                    isMarkdown = true;

                    sb.AppendLine("\r![](https://raw.githubusercontent.com/Sonarr/Sonarr/develop/Logo/128.png)");
                }

                var payload = new GotifyMessage
                {
                    Title = title,
                    Message = sb.ToString(),
                    Priority = Settings.Priority
                };

                payload.SetContentType(isMarkdown);

                _proxy.SendNotification(payload, Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                failures.Add(new ValidationFailure("", "Unable to send test message"));
            }

            return new ValidationResult(failures);
        }

        private void SendNotification(string title, string message, Series series)
        {
            var isMarkdown = false;
            var sb = new StringBuilder();

            sb.AppendLine(message);

            if (Settings.IncludeSeriesPoster && series != null)
            {
                var poster = series.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl;

                if (poster != null)
                {
                    isMarkdown = true;
                    sb.AppendLine($"\r![]({poster})");
                }
            }

            var payload = new GotifyMessage
            {
                Title = title,
                Message = sb.ToString(),
                Priority = Settings.Priority
            };

            payload.SetContentType(isMarkdown);

            _proxy.SendNotification(payload, Settings);
        }
    }
}
