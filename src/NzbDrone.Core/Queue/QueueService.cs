using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Queue
{
    public interface IQueueService
    {
        List<Queue> GetQueue();
        Queue Find(int id);
    }

    public class QueueService : IQueueService, IHandle<TrackedDownloadRefreshedEvent>
    {
        private static List<Queue> _queue = new List<Queue>();
        private readonly IEventAggregator _eventAggregator;

        public QueueService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void Handle(TrackedDownloadRefreshedEvent message)
        {
            _queue = message.TrackedDownloads.OrderBy(c => c.DownloadItem.RemainingTime).SelectMany(MapQueue)
                .ToList();

            _eventAggregator.PublishEvent(new QueueUpdatedEvent());
        }

        public List<Queue> GetQueue()
        {
            return _queue;
        }

        public Queue Find(int id)
        {
            return _queue.SingleOrDefault(q => q.Id == id);
        }

        private IEnumerable<Queue> MapQueue(TrackedDownload trackedDownload)
        {
            if (trackedDownload.RemoteItem is RemoteEpisode)
            {
                var episodes = (trackedDownload.RemoteItem as RemoteEpisode).Episodes;
                if (episodes != null && episodes.Any())
                {
                    foreach (var episode in episodes)
                    {
                        yield return MapEpisode(trackedDownload, episode);
                    }
                }
            } else if (trackedDownload.RemoteItem is RemoteMovie)
            {
                yield return MapMovie(trackedDownload);
            }
        }

        private Queue MapMovie(TrackedDownload trackedDownload)
        {
            var queue = new MovieQueue
            {
                Id = HashConverter.GetHashInt31(string.Format("trackedDownload-{0}-ep0", trackedDownload.DownloadItem.DownloadId)),
                Media = trackedDownload.RemoteItem.Media,
                Quality = trackedDownload.RemoteItem.ParsedInfo.Quality,
                Title = trackedDownload.DownloadItem.Title,
                Size = trackedDownload.DownloadItem.TotalSize,
                Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                Timeleft = trackedDownload.DownloadItem.RemainingTime,
                Status = trackedDownload.DownloadItem.Status.ToString(),
                TrackedDownloadStatus = trackedDownload.Status.ToString(),
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                RemoteItem = trackedDownload.RemoteItem,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                Protocol = trackedDownload.Protocol
            };
            if (queue.Timeleft.HasValue)
            {
                queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.Timeleft.Value);
            }

            return queue;
        }

        private Queue MapEpisode(TrackedDownload trackedDownload, Episode episode)
        {
            var queue = new SeriesQueue
            {
                Id = HashConverter.GetHashInt31(string.Format("trackedDownload-{0}-ep{1}", trackedDownload.DownloadItem.DownloadId, episode.Id)),
                Media = trackedDownload.RemoteItem.Media,
                Episode = episode,
                Quality = trackedDownload.RemoteItem.ParsedInfo.Quality,
                Title = trackedDownload.DownloadItem.Title,
                Size = trackedDownload.DownloadItem.TotalSize,
                Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                Timeleft = trackedDownload.DownloadItem.RemainingTime,
                Status = trackedDownload.DownloadItem.Status.ToString(),
                TrackedDownloadStatus = trackedDownload.Status.ToString(),
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                RemoteItem = trackedDownload.RemoteItem,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                Protocol = trackedDownload.Protocol
            };

            if (queue.Timeleft.HasValue)
            {
                queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.Timeleft.Value);
            }

            return queue;
        }
    }
}
