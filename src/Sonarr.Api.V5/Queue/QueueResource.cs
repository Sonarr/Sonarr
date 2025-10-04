using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using Sonarr.Api.V5.CustomFormats;
using Sonarr.Api.V5.Episodes;
using Sonarr.Api.V5.Series;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Queue
{
    public class QueueResource : RestResource
    {
        public int? SeriesId { get; set; }
        public IEnumerable<int> EpisodeIds { get; set; } = [];
        public List<int> SeasonNumbers { get; set; } = [];
        public SeriesResource? Series { get; set; }
        public List<EpisodeResource>? Episodes { get; set; }
        public List<Language> Languages { get; set; } = [];
        public QualityModel Quality { get; set; } = new(NzbDrone.Core.Qualities.Quality.Unknown);
        public List<CustomFormatResource> CustomFormats { get; set; } = [];
        public int CustomFormatScore { get; set; }
        public decimal Size { get; set; }
        public string? Title { get; set; }
        public decimal SizeLeft { get; set; }
        public TimeSpan? TimeLeft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage>? StatusMessages { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string? DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string? Indexer { get; set; }
        public string? OutputPath { get; set; }
        public int EpisodesWithFilesCount { get; set; }
        public bool IsFullSeason { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this NzbDrone.Core.Queue.Queue model, bool includeSeries, bool includeEpisodes)
        {
            var customFormats = model.RemoteEpisode?.CustomFormats;
            var customFormatScore = model.Series?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new QueueResource
            {
                Id = model.Id,
                SeriesId = model.Series?.Id,
                EpisodeIds = model.Episodes?.Select(e => e.Id).ToList() ?? [],
                SeasonNumbers = model.SeasonNumber.HasValue ? [model.SeasonNumber.Value] : [],
                Series = includeSeries && model.Series != null ? model.Series.ToResource() : null,
                Episodes = includeEpisodes ? model.Episodes?.ToResource() : null,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = customFormats?.ToResource(false) ?? [],
                CustomFormatScore = customFormatScore,
                Size = model.Size,
                Title = model.Title,
                SizeLeft = model.SizeLeft,
                TimeLeft = model.TimeLeft,
                EstimatedCompletionTime = model.EstimatedCompletionTime,
                Added = model.Added,
                Status = model.Status,
                TrackedDownloadStatus = model.TrackedDownloadStatus,
                TrackedDownloadState = model.TrackedDownloadState,
                StatusMessages = model.StatusMessages,
                ErrorMessage = model.ErrorMessage,
                DownloadId = model.DownloadId,
                Protocol = model.Protocol,
                DownloadClient = model.DownloadClient,
                DownloadClientHasPostImportCategory = model.DownloadClientHasPostImportCategory,
                Indexer = model.Indexer,
                OutputPath = model.OutputPath,
                EpisodesWithFilesCount = model.Episodes?.Count(e => e.HasFile) ?? 0,
                IsFullSeason = model.RemoteEpisode?.ParsedEpisodeInfo?.FullSeason ?? false
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<NzbDrone.Core.Queue.Queue> models, bool includeSeries, bool includeEpisode)
        {
            return models.Select((m) => ToResource(m, includeSeries, includeEpisode)).ToList();
        }
    }
}
