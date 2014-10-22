using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.Queue
{
    public interface IQueueService
    {
        List<Queue> GetQueue();
    }

    public class QueueService : IQueueService
    {
        private readonly IDownloadTrackingService _downloadTrackingService;

        public QueueService(IDownloadTrackingService downloadTrackingService)
        {
            _downloadTrackingService = downloadTrackingService;
        }

        public List<Queue> GetQueue()
        {
            var queueItems = _downloadTrackingService.GetQueuedDownloads()
                .OrderBy(v => v.DownloadItem.RemainingTime)
                .ToList();

            return MapQueue(queueItems);
        }

        private List<Queue> MapQueue(IEnumerable<TrackedDownload> trackedDownloads)
        {
            var queued = new List<Queue>();

            foreach (var trackedDownload in trackedDownloads)
            {
                foreach (var episode in trackedDownload.RemoteEpisode.Episodes)
                {
                    var queue = new Queue
                                {
                                    Id = episode.Id ^ (trackedDownload.DownloadItem.DownloadClientId.GetHashCode() << 16),
                                    Series = trackedDownload.RemoteEpisode.Series,
                                    Episode = episode,
                                    Quality = trackedDownload.RemoteEpisode.ParsedEpisodeInfo.Quality,
                                    Title = trackedDownload.DownloadItem.Title,
                                    Size = trackedDownload.DownloadItem.TotalSize,
                                    Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                                    Timeleft = trackedDownload.DownloadItem.RemainingTime,
                                    Status = trackedDownload.DownloadItem.Status.ToString(),
                                    RemoteEpisode = trackedDownload.RemoteEpisode,
                                    TrackedDownloadStatus = trackedDownload.Status.ToString(),
                                    StatusMessages = trackedDownload.StatusMessages,
                                    TrackingId = trackedDownload.TrackingId
                                };

                    if (queue.Timeleft.HasValue)
                    {
                        queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.Timeleft.Value);
                    }

                    queued.Add(queue);
                }
            }

            return queued;
        }
    }
}
