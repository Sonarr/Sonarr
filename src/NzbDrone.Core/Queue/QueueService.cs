using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;

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
            var queueItems = downloadClient.GetQueue();

            return MapQueue(queueItems);
        }

        private List<Queue> MapQueue(IEnumerable<QueueItem> queueItems)
        {
            var queued = new List<Queue>();

            foreach (var queueItem in queueItems)
            {
                foreach (var episode in queueItem.RemoteEpisode.Episodes)
                {
                    var queue = new Queue();
                    queue.Id = queueItem.Id.GetHashCode();
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
