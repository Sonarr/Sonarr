using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using Sonarr.Api.V3.CustomFormats;
using Sonarr.Api.V3.Episodes;
using Sonarr.Api.V3.Series;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Queue
{
    public class QueueResource : RestResource
    {
        public int? SeriesId { get; set; }
        public int? EpisodeId { get; set; }
        public int? SeasonNumber { get; set; }
        public SeriesResource Series { get; set; }
        public EpisodeResource Episode { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }

        // Collides with existing properties due to case-insensitive deserialization
        // public decimal SizeLeft { get; set; }
        // public TimeSpan? TimeLeft { get; set; }

        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public string ErrorMessage { get; set; }
        public string DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string Indexer { get; set; }
        public string OutputPath { get; set; }
        public bool EpisodeHasFile { get; set; }

        [Obsolete("Will be replaced by SizeLeft")]
        public decimal Sizeleft { get; set; }

        [Obsolete("Will be replaced by TimeLeft")]
        public TimeSpan? Timeleft { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this NzbDrone.Core.Queue.Queue model, bool includeSeries, bool includeEpisode)
        {
            if (model == null)
            {
                return null;
            }

            var customFormats = model.RemoteEpisode?.CustomFormats;
            var customFormatScore = model.Series?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new QueueResource
            {
                Id = model.Id,
                SeriesId = model.Series?.Id,
                EpisodeId = model.Episode?.Id,
                SeasonNumber = model.Episode?.SeasonNumber,
                Series = includeSeries && model.Series != null ? model.Series.ToResource() : null,
                Episode = includeEpisode && model.Episode != null ? model.Episode.ToResource() : null,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = customFormats?.ToResource(false),
                CustomFormatScore = customFormatScore,
                Size = model.Size,
                Title = model.Title,

                // Collides with existing properties due to case-insensitive deserialization
                // SizeLeft = model.SizeLeft,
                // TimeLeft = model.TimeLeft,

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
                EpisodeHasFile = model.Episode?.HasFile ?? false,

                #pragma warning disable CS0618
                Sizeleft = model.SizeLeft,
                Timeleft = model.TimeLeft,
                #pragma warning restore CS0618
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<NzbDrone.Core.Queue.Queue> models, bool includeSeries, bool includeEpisode)
        {
            return models.Select((m) => ToResource(m, includeSeries, includeEpisode)).ToList();
        }
    }
}
