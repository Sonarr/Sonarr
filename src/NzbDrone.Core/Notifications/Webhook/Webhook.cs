using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.Tv;
using NzbDrone.Common.Extensions;
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

        public override string Link => "https://github.com/Sonarr/Sonarr/wiki/Webhook";

        public override void OnGrab(GrabMessage message)
        {
            var remoteEpisode = message.Episode;
            var quality = message.Quality;

            var payload = new WebhookGrabPayload
            {
                EventType = "Grab",
                Series = new WebhookSeries(message.Series),
                Episodes = remoteEpisode.Episodes.ConvertAll(x => new WebhookEpisode(x)
                {
                    // TODO: Stop passing these parameters inside an episode v3
                    Quality = quality.Quality.Name,
                    QualityVersion = quality.Revision.Version,
                    ReleaseGroup = remoteEpisode.ParsedEpisodeInfo.ReleaseGroup
                }),
                Release = new WebhookRelease(quality, remoteEpisode),
                DownloadClient = message.DownloadClient,
                DownloadId = message.DownloadId
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var episodeFile = message.EpisodeFile;

            var payload = new WebhookImportPayload
            {
                EventType = "Download",
                Series = new WebhookSeries(message.Series),
                Episodes = episodeFile.Episodes.Value.ConvertAll(x => new WebhookEpisode(x)
                {
                    // TODO: Stop passing these parameters inside an episode v3
                    Quality = episodeFile.Quality.Quality.Name,
                    QualityVersion = episodeFile.Quality.Revision.Version,
                    ReleaseGroup = episodeFile.ReleaseGroup,
                    SceneName = episodeFile.SceneName
                }),
                EpisodeFile = new WebhookEpisodeFile(episodeFile),
                IsUpgrade = message.OldFiles.Any(),
                DownloadClient = message.DownloadClient,
                DownloadId = message.DownloadId
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnRename(Series series)
        {
            var payload = new WebhookPayload
            {
                EventType = "Rename",
                Series = new WebhookSeries(series)
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
                        EventType = "Test",
                        Series = new WebhookSeries()
                        {
                            Id = 1,
                            Title = "Test Title",
                            Path = "C:\\testpath",
                            TvdbId = 1234
                        },
                        Episodes = new List<WebhookEpisode>() {
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
