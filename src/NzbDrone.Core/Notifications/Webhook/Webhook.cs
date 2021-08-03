using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class Webhook : NotificationBase<WebhookSettings>
    {
        private readonly IWebhookProxy _proxy;

        public Webhook(IWebhookProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://wiki.servarr.com/sonarr/settings#connections";

        public override void OnGrab(GrabMessage message)
        {
            var remoteEpisode = message.Episode;
            var quality = message.Quality;

            var payload = new WebhookGrabPayload
            {
                EventType = WebhookEventType.Grab,
                Series = new WebhookSeries(message.Series),
                Episodes = remoteEpisode.Episodes.ConvertAll(x => new WebhookEpisode(x)),
                Release = new WebhookRelease(quality, remoteEpisode),
                DownloadClient = message.DownloadClientName,
                DownloadClientType = message.DownloadClientType,
                DownloadId = message.DownloadId
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var episodeFile = message.EpisodeFile;

            var payload = new WebhookImportPayload
            {
                EventType = WebhookEventType.Download,
                Series = new WebhookSeries(message.Series),
                Episodes = episodeFile.Episodes.Value.ConvertAll(x => new WebhookEpisode(x)),
                EpisodeFile = new WebhookEpisodeFile(episodeFile),
                IsUpgrade = message.OldFiles.Any(),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId
            };

            if (message.OldFiles.Any())
            {
                payload.DeletedFiles = message.OldFiles.ConvertAll(x => new WebhookEpisodeFile(x)
                {
                    Path = Path.Combine(message.Series.Path, x.RelativePath)
                });
            }

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            var payload = new WebhookRenamePayload
            {
                EventType = WebhookEventType.Rename,
                Series = new WebhookSeries(series),
                RenamedEpisodeFiles = renamedFiles.ConvertAll(x => new WebhookRenamedEpisodeFile(x))
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var payload = new WebhookEpisodeDeletePayload
            {
                EventType = WebhookEventType.EpisodeFileDelete,
                Series = new WebhookSeries(deleteMessage.Series),
                Episodes = deleteMessage.EpisodeFile.Episodes.Value.ConvertAll(x => new WebhookEpisode(x)),
                EpisodeFile = deleteMessage.EpisodeFile,
                DeleteReason = deleteMessage.Reason
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var payload = new WebhookSeriesDeletePayload
            {
                EventType = WebhookEventType.SeriesDelete,
                Series = new WebhookSeries(deleteMessage.Series),
                DeletedFiles = deleteMessage.DeletedFiles
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var payload = new WebhookHealthPayload
            {
                EventType = WebhookEventType.Health,
                Level = healthCheck.Type,
                Message = healthCheck.Message,
                Type = healthCheck.Source.Name,
                WikiUrl = healthCheck.WikiUrl?.ToString()
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var payload = new WebhookApplicationUpdatePayload
            {
                EventType = WebhookEventType.ApplicationUpdate,
                Message = updateMessage.Message,
                PreviousVersion = updateMessage.PreviousVersion.ToString(),
                NewVersion = updateMessage.NewVersion.ToString()
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override string Name => "Webhook";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(SendWebhookTest());

            return new ValidationResult(failures);
        }

        private ValidationFailure SendWebhookTest()
        {
            try
            {
                var payload = new WebhookGrabPayload
                {
                    EventType = WebhookEventType.Test,
                    Series = new WebhookSeries()
                    {
                        Id = 1,
                        Title = "Test Title",
                        Path = "C:\\testpath",
                        TvdbId = 1234
                    },
                    Episodes = new List<WebhookEpisode>()
                    {
                        new WebhookEpisode()
                        {
                            Id = 123,
                            EpisodeNumber = 1,
                            SeasonNumber = 1,
                            Title = "Test title"
                        }
                    }
                };

                _proxy.SendWebhook(payload, Settings);
            }
            catch (WebhookException ex)
            {
                return new NzbDroneValidationFailure("Url", ex.Message);
            }

            return null;
        }
    }
}
