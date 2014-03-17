using System;
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
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;

        public QueueService(IProvideDownloadClient downloadClientProvider, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public List<Queue> GetQueue()
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (downloadClient == null)
            {
                _logger.Debug("Download client is not configured.");
                return new List<Queue>();
            }

            try
            {
                var queueItems = downloadClient.GetQueue();

                return MapQueue(queueItems);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting queue from download client: " + downloadClient.ToString(), ex);
                return new List<Queue>();
            }
        }

        private List<Queue> MapQueue(IEnumerable<QueueItem> queueItems)
        {
            var queued = new List<Queue>();

            foreach (var queueItem in queueItems)
            {
                foreach (var episode in queueItem.RemoteEpisode.Episodes)
                {
                    var queue = new Queue();
                    queue.Id = queueItem.Id.GetHashCode() + episode.Id;
                    queue.Series = queueItem.RemoteEpisode.Series;
                    queue.Episode = episode;
                    queue.Quality = queueItem.RemoteEpisode.ParsedEpisodeInfo.Quality;
                    queue.Title = queueItem.Title;
                    queue.Size = queueItem.Size;
                    queue.Sizeleft = queueItem.Sizeleft;
                    queue.Timeleft = queueItem.Timeleft;
                    queue.Status = queueItem.Status;
                    queued.Add(queue);
                }
            }

            return queued;
        }
    }
}
