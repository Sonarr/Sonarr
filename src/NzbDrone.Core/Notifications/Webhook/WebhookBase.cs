using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Webhook
{
    public abstract class WebhookBase<TSettings> : NotificationBase<TSettings>
        where TSettings : NotificationSettingsBase<TSettings>, new()
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        protected readonly ILocalizationService _localizationService;
        private readonly ITagRepository _tagRepository;
        private readonly IMapCoversToLocal _mediaCoverService;

        protected WebhookBase(IConfigFileProvider configFileProvider, IConfigService configService, ILocalizationService localizationService, ITagRepository tagRepository, IMapCoversToLocal mediaCoverService)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _localizationService = localizationService;
            _tagRepository = tagRepository;
            _mediaCoverService = mediaCoverService;
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
                Series = GetSeries(message.Series),
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
                Series = GetSeries(message.Series),
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
                payload.DeletedFiles = message.OldFiles.ConvertAll(x => new WebhookEpisodeFile(x.EpisodeFile)
                {
                    Path = Path.Combine(message.Series.Path, x.EpisodeFile.RelativePath),
                    RecycleBinPath = x.RecycleBinPath
                });
            }

            return payload;
        }

        protected WebhookImportCompletePayload BuildOnImportCompletePayload(ImportCompleteMessage message)
        {
            var episodeFiles = message.EpisodeFiles;

            var payload = new WebhookImportCompletePayload
            {
                EventType = WebhookEventType.Download,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = GetSeries(message.Series),
                Episodes = message.Episodes.ConvertAll(x => new WebhookEpisode(x)),
                EpisodeFiles = episodeFiles.ConvertAll(e => new WebhookEpisodeFile(e)),
                Release = new WebhookGrabbedRelease(message.Release, episodeFiles.First().ReleaseType),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                SourcePath = message.SourcePath,
                DestinationPath = message.DestinationPath
            };

            return payload;
        }

        protected WebhookEpisodeDeletePayload BuildOnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            return new WebhookEpisodeDeletePayload
            {
                EventType = WebhookEventType.EpisodeFileDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = GetSeries(deleteMessage.Series),
                Episodes = deleteMessage.EpisodeFile.Episodes.Value.ConvertAll(x => new WebhookEpisode(x)),
                EpisodeFile = new WebhookEpisodeFile(deleteMessage.EpisodeFile),
                DeleteReason = deleteMessage.Reason
            };
        }

        protected WebhookSeriesAddPayload BuildOnSeriesAdd(SeriesAddMessage addMessage)
        {
            return new WebhookSeriesAddPayload
            {
                EventType = WebhookEventType.SeriesAdd,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = GetSeries(addMessage.Series),
            };
        }

        protected WebhookSeriesDeletePayload BuildOnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            return new WebhookSeriesDeletePayload
            {
                EventType = WebhookEventType.SeriesDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = GetSeries(deleteMessage.Series),
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
                Series = GetSeries(series),
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

        protected WebhookHealthPayload BuildHealthRestoredPayload(HealthCheck.HealthCheck healthCheck)
        {
            return new WebhookHealthPayload
            {
                EventType = WebhookEventType.HealthRestored,
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

        protected WebhookManualInteractionPayload BuildManualInteractionRequiredPayload(ManualInteractionRequiredMessage message)
        {
            var remoteEpisode = message.Episode;
            var quality = message.Quality;

            return new WebhookManualInteractionPayload
            {
                EventType = WebhookEventType.ManualInteractionRequired,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = GetSeries(message.Series),
                Episodes = remoteEpisode.Episodes.ConvertAll(x => new WebhookEpisode(x)),
                DownloadInfo = new WebhookDownloadClientItem(quality, message.TrackedDownload.DownloadItem),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                DownloadStatus = message.TrackedDownload.Status.ToString(),
                DownloadStatusMessages = message.TrackedDownload.StatusMessages.Select(x => new WebhookDownloadStatusMessage(x)).ToList(),
                CustomFormatInfo = new WebhookCustomFormatInfo(remoteEpisode.CustomFormats, remoteEpisode.CustomFormatScore),
                Release = new WebhookGrabbedRelease(message.Release)
            };
        }

        protected WebhookPayload BuildTestPayload()
        {
            return new WebhookGrabPayload
            {
                EventType = WebhookEventType.Test,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Series = new WebhookSeries
                {
                    Id = 1,
                    Title = "Test Title",
                    Path = "C:\\testpath",
                    TvdbId = 1234,
                    Tags = new List<string> { "test-tag" }
                },
                Episodes = new List<WebhookEpisode>
                {
                    new ()
                    {
                        Id = 123,
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        Title = "Test title"
                    }
                }
            };
        }

        private WebhookSeries GetSeries(Series series)
        {
            if (series == null)
            {
                return null;
            }

            _mediaCoverService.ConvertToLocalUrls(series.Id, series.Images);

            return new WebhookSeries(series, GetTagLabels(series));
        }

        private List<string> GetTagLabels(Series series)
        {
            if (series == null)
            {
                return null;
            }

            return _tagRepository.GetTags(series.Tags)
                .Select(s => s.Label)
                .Where(l => l.IsNotNullOrWhiteSpace())
                .OrderBy(l => l)
                .ToList();
        }
    }
}
