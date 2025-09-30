using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IFailedDownloadService
    {
        void MarkAsFailed(int historyId, string message = null, string source = null, bool skipRedownload = false);
        void MarkAsFailed(TrackedDownload trackedDownload, string message = null, string source = null, bool skipRedownload = false);
        void Check(TrackedDownload trackedDownload);
        void ProcessFailed(TrackedDownload trackedDownload);
    }

    public class FailedDownloadService : IFailedDownloadService
    {
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;

        public FailedDownloadService(IHistoryService historyService,
                                     ITrackedDownloadService trackedDownloadService,
                                     IEventAggregator eventAggregator)
        {
            _historyService = historyService;
            _eventAggregator = eventAggregator;
        }

        public void MarkAsFailed(int historyId, string message, string source = null, bool skipRedownload = false)
        {
            message ??= "Manually marked as failed";

            var history = _historyService.Get(historyId);
            var downloadId = history.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                PublishDownloadFailedEvent(history, new List<int> { history.EpisodeId }, message, source, skipRedownload: skipRedownload);

                return;
            }

            var grabbedHistory = new List<EpisodeHistory>();

            // If the history item is a grabbed item (it should be, at least from the UI) add it as the first history item
            if (history.EventType == EpisodeHistoryEventType.Grabbed)
            {
                grabbedHistory.Add(history);
            }

            // Add any other history items for the download ID then filter out any duplicate history items.
            grabbedHistory.AddRange(GetGrabbedHistory(downloadId));
            grabbedHistory = grabbedHistory.DistinctBy(h => h.Id).ToList();

            PublishDownloadFailedEvent(history, GetEpisodeIds(grabbedHistory), message, source);
        }

        public void MarkAsFailed(TrackedDownload trackedDownload, string message, string source = null, bool skipRedownload = false)
        {
            var history = GetGrabbedHistory(trackedDownload.DownloadItem.DownloadId);

            if (history.Any())
            {
                PublishDownloadFailedEvent(history.First(), GetEpisodeIds(history), message ?? "Manually marked as failed", source, trackedDownload, skipRedownload: skipRedownload);
            }
        }

        public void Check(TrackedDownload trackedDownload)
        {
            // Only process tracked downloads that are still downloading or import is blocked (if they fail after attempting to be processed)
            if (trackedDownload.State != TrackedDownloadState.Downloading && trackedDownload.State != TrackedDownloadState.ImportBlocked)
            {
                return;
            }

            if (trackedDownload.DownloadItem.IsEncrypted ||
                trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed)
            {
                var grabbedItems = GetGrabbedHistory(trackedDownload.DownloadItem.DownloadId);

                if (grabbedItems.Empty())
                {
                    trackedDownload.Warn(trackedDownload.DownloadItem.IsEncrypted ? "Download is encrypted and wasn't grabbed by Sonarr, skipping automatic download handling" : "Download has failed wasn't grabbed by Sonarr, skipping automatic download handling");
                    return;
                }

                trackedDownload.State = TrackedDownloadState.FailedPending;
            }
        }

        public void ProcessFailed(TrackedDownload trackedDownload)
        {
            if (trackedDownload.State != TrackedDownloadState.FailedPending)
            {
                return;
            }

            var grabbedItems = GetGrabbedHistory(trackedDownload.DownloadItem.DownloadId);

            if (grabbedItems.Empty())
            {
                return;
            }

            var failure = "Failed download detected";

            if (trackedDownload.DownloadItem.IsEncrypted)
            {
                failure = "Encrypted download detected";
            }
            else if (trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed && trackedDownload.DownloadItem.Message.IsNotNullOrWhiteSpace())
            {
                failure = trackedDownload.DownloadItem.Message;
            }

            trackedDownload.State = TrackedDownloadState.Failed;
            PublishDownloadFailedEvent(grabbedItems.First(), GetEpisodeIds(grabbedItems), failure, $"{BuildInfo.AppName} Failed Download Handling", trackedDownload);
        }

        private void PublishDownloadFailedEvent(EpisodeHistory historyItem, List<int> episodeIds, string message, string source, TrackedDownload trackedDownload = null, bool skipRedownload = false)
        {
            Enum.TryParse(historyItem.Data.GetValueOrDefault(EpisodeHistory.RELEASE_SOURCE, ReleaseSourceType.Unknown.ToString()), out ReleaseSourceType releaseSource);

            var downloadFailedEvent = new DownloadFailedEvent
            {
                SeriesId = historyItem.SeriesId,
                EpisodeIds = episodeIds,
                Quality = historyItem.Quality,
                SourceTitle = historyItem.SourceTitle,
                DownloadClient = historyItem.Data.GetValueOrDefault(EpisodeHistory.DOWNLOAD_CLIENT),
                DownloadId = historyItem.DownloadId,
                Message = message,
                Source = source,
                Data = historyItem.Data,
                TrackedDownload = trackedDownload,
                Languages = historyItem.Languages,
                SkipRedownload = skipRedownload,
                ReleaseSource = releaseSource,
            };

            _eventAggregator.PublishEvent(downloadFailedEvent);
        }

        private List<int> GetEpisodeIds(List<EpisodeHistory> historyItems)
        {
            return historyItems.Select(h => h.EpisodeId).Distinct().ToList();
        }

        private List<EpisodeHistory> GetGrabbedHistory(string downloadId)
        {
            // Sort by date so items are always in the same order
            return _historyService.Find(downloadId, EpisodeHistoryEventType.Grabbed)
                .OrderByDescending(h => h.Date)
                .ToList();
        }
    }
}
