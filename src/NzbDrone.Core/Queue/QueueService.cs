using System;
using System.Linq;
using System.Collections.Generic;
using NLog;
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
        private readonly Logger _logger;

        public QueueService(IDownloadTrackingService downloadTrackingService, Logger logger)
        {
            _downloadTrackingService = downloadTrackingService;
            _logger = logger;
        }

        public List<Queue> GetQueue()
        {
            var queueItems = _downloadTrackingService.GetQueuedDownloads()
                .OrderBy(v => v.DownloadItem.RemainingTime)
                .ToList();

            return MapQueue(queueItems);
        }

        private List<Queue> MapQueue(IEnumerable<TrackedDownload> queueItems)
        {
            var queued = new List<Queue>();

            foreach (var queueItem in queueItems)
            {
                foreach (var episode in queueItem.DownloadItem.RemoteEpisode.Episodes)
                {
                    var queue = new Queue();
                    queue.Id = queueItem.DownloadItem.DownloadClientId.GetHashCode() + episode.Id;
                    queue.Series = queueItem.DownloadItem.RemoteEpisode.Series;
                    queue.Episode = episode;
                    queue.Quality = queueItem.DownloadItem.RemoteEpisode.ParsedEpisodeInfo.Quality;
                    queue.Title = queueItem.DownloadItem.Title;
                    queue.Size = queueItem.DownloadItem.TotalSize;
                    queue.Sizeleft = queueItem.DownloadItem.RemainingSize;
                    queue.Timeleft = queueItem.DownloadItem.RemainingTime;
                    queue.Status = queueItem.DownloadItem.Status.ToString();
                    queue.RemoteEpisode = queueItem.DownloadItem.RemoteEpisode;

                    if (queueItem.HasError)
                    {
                        queue.ErrorMessage = queueItem.StatusMessage;
                    }

                    queued.Add(queue);
                }
            }

            return queued;
        }
    }
}
