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
                .Select(v => v.DownloadItem)
                .OrderBy(v => v.RemainingTime)
                .ToList();

            return MapQueue(queueItems);
        }

        private List<Queue> MapQueue(IEnumerable<DownloadClientItem> queueItems)
        {
            var queued = new List<Queue>();

            foreach (var queueItem in queueItems)
            {
                foreach (var episode in queueItem.RemoteEpisode.Episodes)
                {
                    var queue = new Queue();
                    queue.Id = queueItem.DownloadClientId.GetHashCode() + episode.Id;
                    queue.Series = queueItem.RemoteEpisode.Series;
                    queue.Episode = episode;
                    queue.Quality = queueItem.RemoteEpisode.ParsedEpisodeInfo.Quality;
                    queue.Title = queueItem.Title;
                    queue.Size = queueItem.TotalSize;
                    queue.Sizeleft = queueItem.RemainingSize;
                    queue.Timeleft = queueItem.RemainingTime;
                    queue.Status = queueItem.Status.ToString();
                    queue.RemoteEpisode = queueItem.RemoteEpisode;
                    queued.Add(queue);
                }
            }

            return queued;
        }
    }
}
