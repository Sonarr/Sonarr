using System;
using System.Collections.Generic;
using Sonarr.Http.REST;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Series;
using NzbDrone.Api.Episodes;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Indexers;
using System.Linq;

namespace NzbDrone.Api.Queue
{
    public class QueueResource : RestResource
    {
        public SeriesResource Series { get; set; }
        public EpisodeResource Episode { get; set; }
        public QualityModel Quality { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }
        public decimal Sizeleft { get; set; }
        public TimeSpan? Timeleft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public string Status { get; set; }
        public string TrackedDownloadStatus { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public string DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this Core.Queue.Queue model)
        {
            if (model == null) return null;

            return new QueueResource
            {
                Id = model.Id,

                Series = model.Series.ToResource(),
                Episode = model.Episode.ToResource(),
                Quality = model.Quality,
                Size = model.Size,
                Title = model.Title,
                Sizeleft = model.Sizeleft,
                Timeleft = model.Timeleft,
                EstimatedCompletionTime = model.EstimatedCompletionTime,
                Status = model.Status,
                TrackedDownloadStatus = model.TrackedDownloadStatus,
                StatusMessages = model.StatusMessages,
                DownloadId = model.DownloadId,
                Protocol = model.Protocol
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<Core.Queue.Queue> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
