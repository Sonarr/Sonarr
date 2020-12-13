using System.Collections.Generic;
using System.IO;
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

        public override string Link => "https://wiki.servarr.com/Sonarr_Settings#Connections";

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
                EventType = WebhookEventType.Download,
                Series = new WebhookSeries(message.Series),
                Episodes = episodeFile.Episodes.Value.ConvertAll(x => new WebhookEpisode(x)),
                EpisodeFile = new WebhookEpisodeFile(episodeFile),
                IsUpgrade = message.OldFiles.Any(),
                DownloadClient = message.DownloadClient,
                DownloadId = message.DownloadId
            };

            if (message.OldFiles.Any())
            {
                payload.DeletedFiles = message.OldFiles.ConvertAll(x => new WebhookEpisodeFile(x)
                                                                        {
                                                                            Path = Path.Combine(message.Series.Path,
                                                                                x.RelativePath)
                                                                        }
                );
            }

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnRename(Series series)
        {
            var payload = new WebhookRenamePayload
            {
                EventType = WebhookEventType.Rename,
                Series = new WebhookSeries(series)
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
