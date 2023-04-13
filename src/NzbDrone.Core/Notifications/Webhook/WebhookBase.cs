using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Webhook
{
    public abstract class WebhookBase<TSettings> : NotificationBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;

        protected WebhookBase(IConfigFileProvider configFileProvider, IConfigService configService)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
        }

        protected WebhookGrabPayload BuildOnGrabPayload(GrabMessage message)
        {
            var remoteEpisode = message.Episode;
            var quality = message.Quality;

            return new WebhookGrabPayload
            {
                EventType = WebhookEventType.Grab,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = new WebhookSeries(message.Series),
                Episodes = remoteEpisode.Episodes.ConvertAll(x => new WebhookEpisode(x)),
                Release = new WebhookRelease(quality, remoteEpisode),
                DownloadClient = message.DownloadClientName,
                DownloadClientType = message.DownloadClientType,
                DownloadId = message.DownloadId,
                CustomFormatInfo = new WebhookCustomFormatInfo(remoteEpisode.CustomFormats, remoteEpisode.CustomFormatScore),
            };
        }

        protected WebhookImportPayload BuildOnDownloadPayload(DownloadMessage message)
        {
            var episodeFile = message.EpisodeFile;

            var payload = new WebhookImportPayload
            {
                EventType = WebhookEventType.Download,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = new WebhookSeries(message.Series),
                Episodes = episodeFile.Episodes.Value.ConvertAll(x => new WebhookEpisode(x)),
                EpisodeFile = new WebhookEpisodeFile(episodeFile),
                Release = new WebhookGrabbedRelease(message.Release),
                IsUpgrade = message.OldFiles.Any(),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                CustomFormatInfo = new WebhookCustomFormatInfo(message.EpisodeInfo.CustomFormats, message.EpisodeInfo.CustomFormatScore)
            };

            if (message.OldFiles.Any())
            {
                payload.DeletedFiles = message.OldFiles.ConvertAll(x => new WebhookEpisodeFile(x)
                {
                    Path = Path.Combine(message.Series.Path, x.RelativePath)
                });
            }

            return payload;
        }

        protected WebhookEpisodeDeletePayload BuildOnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            return new WebhookEpisodeDeletePayload
            {
                EventType = WebhookEventType.EpisodeFileDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = new WebhookSeries(deleteMessage.Series),
                Episodes = deleteMessage.EpisodeFile.Episodes.Value.ConvertAll(x => new WebhookEpisode(x)),
                EpisodeFile = deleteMessage.EpisodeFile,
                DeleteReason = deleteMessage.Reason
            };
        }

        protected WebhookSeriesDeletePayload BuildOnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            return new WebhookSeriesDeletePayload
            {
                EventType = WebhookEventType.SeriesDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = new WebhookSeries(deleteMessage.Series),
                DeletedFiles = deleteMessage.DeletedFiles
            };
        }

        protected WebhookRenamePayload BuildOnRenamePayload(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            return new WebhookRenamePayload
            {
                EventType = WebhookEventType.Rename,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = new WebhookSeries(series),
                RenamedEpisodeFiles = renamedFiles.ConvertAll(x => new WebhookRenamedEpisodeFile(x))
            };
        }

        protected WebhookHealthPayload BuildHealthPayload(HealthCheck.HealthCheck healthCheck)
        {
            return new WebhookHealthPayload
            {
                EventType = WebhookEventType.Health,
                InstanceName = _configFileProvider.InstanceName,
                Level = healthCheck.Type,
                Message = healthCheck.Message,
                Type = healthCheck.Source.Name,
                WikiUrl = healthCheck.WikiUrl?.ToString()
            };
        }

        protected WebhookApplicationUpdatePayload BuildApplicationUpdatePayload(ApplicationUpdateMessage updateMessage)
        {
            return new WebhookApplicationUpdatePayload
            {
                EventType = WebhookEventType.ApplicationUpdate,
                InstanceName = _configFileProvider.InstanceName,
                Message = updateMessage.Message,
                PreviousVersion = updateMessage.PreviousVersion.ToString(),
                NewVersion = updateMessage.NewVersion.ToString()
            };
        }

        protected WebhookPayload BuildTestPayload()
        {
            return new WebhookGrabPayload
            {
                EventType = WebhookEventType.Test,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
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
        }
    }
}
