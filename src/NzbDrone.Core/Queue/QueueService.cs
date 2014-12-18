using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Queue
{
    public interface IQueueService
    {
        List<Queue> GetQueue();
        Queue Find(int id);
    }

    public class QueueService : IQueueService, IHandle<TrackedDownloadRefreshedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private static List<Queue> _queue = new List<Queue>();

        public QueueService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public List<Queue> GetQueue()
        {
            return _queue;
        }

        public Queue Find(int id)
        {
            return _queue.SingleOrDefault(q => q.Id == id);
        }

        public void Handle(TrackedDownloadRefreshedEvent message)
        {
            _queue = message.TrackedDownloads.OrderBy(c => c.DownloadItem.RemainingTime).SelectMany(MapQueue)
                .ToList();

            _eventAggregator.PublishEvent(new QueueUpdatedEvent());
        }

        private static IEnumerable<Queue> MapQueue(TrackedDownload trackedDownload)
        {
            foreach (var episode in trackedDownload.RemoteEpisode.Episodes)
            {
                var queue = new Queue
                {
                    Id = episode.Id ^ (trackedDownload.DownloadItem.DownloadId.GetHashCode() << 16),
                    Series = trackedDownload.RemoteEpisode.Series,
                    Episode = episode,
                    Quality = trackedDownload.RemoteEpisode.ParsedEpisodeInfo.Quality,
                    Title = trackedDownload.DownloadItem.Title,
                    Size = trackedDownload.DownloadItem.TotalSize,
                    Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                    Timeleft = trackedDownload.DownloadItem.RemainingTime,
                    Status = trackedDownload.DownloadItem.Status.ToString(),
                    TrackedDownloadStatus = trackedDownload.Status.ToString(),
                    StatusMessages = trackedDownload.StatusMessages.ToList(),
                    RemoteEpisode = trackedDownload.RemoteEpisode,
                    TrackingId = trackedDownload.TrackingId
                };

                if (queue.Timeleft.HasValue)
                {
                    queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.Timeleft.Value);
                }

                yield return queue;
            }

        }
    }
}
