using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public interface IFailedDownloadService
    {
        void MarkAsFailed(int historyId);
        void Process(TrackedDownload trackedDownload);
    }

    public class FailedDownloadService : IFailedDownloadService
    {
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;

        public FailedDownloadService(IHistoryService historyService,
                                     IEventAggregator eventAggregator)
        {
            _historyService = historyService;
            _eventAggregator = eventAggregator;
        }

        public void MarkAsFailed(int historyId)
        {
            var history = _historyService.Get(historyId);

            var downloadId = history.DownloadId;
            if (downloadId.IsNullOrWhiteSpace())
            {
                PublishDownloadFailedEvent(new List<History.History> { history }, "Manually marked as failed");
            }
            else
            {
                var grabbedHistory = _historyService.Find(downloadId, HistoryEventType.Grabbed).ToList();
                PublishDownloadFailedEvent(grabbedHistory, "Manually marked as failed");
            }
        }

        public void Process(TrackedDownload trackedDownload)
        {
            var grabbedItems = _historyService.Find(trackedDownload.DownloadItem.DownloadId, HistoryEventType.Grabbed)
                .ToList();

            if (grabbedItems.Empty())
            {
                trackedDownload.Warn("Download wasn't grabbed by sonarr, skipping");
                return;
            }

            if (trackedDownload.DownloadItem.IsEncrypted)
            {
                trackedDownload.State = TrackedDownloadStage.DownloadFailed;
                PublishDownloadFailedEvent(grabbedItems, "Encrypted download detected");
            }
            else if (trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed)
            {
                trackedDownload.State = TrackedDownloadStage.DownloadFailed;
                PublishDownloadFailedEvent(grabbedItems, trackedDownload.DownloadItem.Message);
            }
        }

        private void PublishDownloadFailedEvent(List<History.History> historyItems, string message)
        {
            var historyItem = historyItems.First();

            var downloadFailedEvent = new DownloadFailedEvent
            {
                SeriesId = historyItem.SeriesId,
                EpisodeIds = historyItems.Select(h => h.EpisodeId).ToList(),
                Quality = historyItem.Quality,
                SourceTitle = historyItem.SourceTitle,
                DownloadClient = historyItem.Data.GetValueOrDefault(History.History.DOWNLOAD_CLIENT),
                DownloadId = historyItem.DownloadId,
                Message = message,
                Data = historyItem.Data
            };

            _eventAggregator.PublishEvent(downloadFailedEvent);
        }


    }
}
