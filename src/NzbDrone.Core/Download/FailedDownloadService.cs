using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public interface IFailedDownloadService
    {
        void MarkAsFailed(int historyId);
        void CheckForFailedItem(IDownloadClient downloadClient, TrackedDownload trackedDownload, List<History.History> grabbedHistory, List<History.History> failedHistory);
    }

    public class FailedDownloadService : IFailedDownloadService
    {
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public FailedDownloadService(IHistoryService historyService,
                                     IEventAggregator eventAggregator,
                                     IConfigService configService,
                                     Logger logger)
        {
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _logger = logger;
        }

        public void MarkAsFailed(int historyId)
        {
            var item = _historyService.Get(historyId);
            PublishDownloadFailedEvent(new List<History.History> { item }, "Manually marked as failed");
        }

        public void CheckForFailedItem(IDownloadClient downloadClient, TrackedDownload trackedDownload, List<History.History> grabbedHistory, List<History.History> failedHistory)
        {
            if (!_configService.EnableFailedDownloadHandling)
            {
                return;
            }

            if (trackedDownload.DownloadItem.IsEncrypted && trackedDownload.State == TrackedDownloadState.Downloading)
            {
                var grabbedItems = GetHistoryItems(grabbedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (!grabbedItems.Any())
                {
                    UpdateStatusMessage(LogLevel.Debug, trackedDownload, "Download was not grabbed by drone, ignoring download");
                    return;
                }

                trackedDownload.State = TrackedDownloadState.DownloadFailed;

                var failedItems = GetHistoryItems(failedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (failedItems.Any())
                {
                    UpdateStatusMessage(LogLevel.Debug, trackedDownload, "Already added to history as failed");
                }
                else
                {
                    PublishDownloadFailedEvent(grabbedItems, "Encrypted download detected");
                }
            }

            if (trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed && trackedDownload.State == TrackedDownloadState.Downloading)
            {
                var grabbedItems = GetHistoryItems(grabbedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (!grabbedItems.Any())
                {
                    UpdateStatusMessage(LogLevel.Debug, trackedDownload, "Download wasn't grabbed by drone or not in a category, ignoring download.");
                    return;
                }

                //TODO: Make this more configurable (ignore failure reasons) to support changes and other failures that should be ignored
                if (trackedDownload.DownloadItem.Message.Equals("Unpacking failed, write error or disk is full?",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    UpdateStatusMessage(LogLevel.Error, trackedDownload, "Download failed due to lack of disk space, not blacklisting.");
                    return;
                }

                if (FailedDownloadForRecentRelease(downloadClient, trackedDownload, grabbedItems))
                {
                    UpdateStatusMessage(LogLevel.Debug, trackedDownload, "Recent release Failed, do not blacklist.");
                    return;
                }

                trackedDownload.State = TrackedDownloadState.DownloadFailed;

                var failedItems = GetHistoryItems(failedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (failedItems.Any())
                {
                    UpdateStatusMessage(LogLevel.Debug, trackedDownload, "Already added to history as failed.");
                }
                else
                {
                    PublishDownloadFailedEvent(grabbedItems, trackedDownload.DownloadItem.Message);
                }
            }

            if (trackedDownload.DownloadItem.Status != DownloadItemStatus.Failed && trackedDownload.State == TrackedDownloadState.Downloading)
            {
                var grabbedItems = GetHistoryItems(grabbedHistory, trackedDownload.DownloadItem.DownloadClientId);
                var failedItems = GetHistoryItems(failedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (grabbedItems.Any() && failedItems.Any())
                {
                    UpdateStatusMessage(LogLevel.Debug, trackedDownload, "Already added to history as failed, updating tracked state.");
                    trackedDownload.State = TrackedDownloadState.DownloadFailed;
                }
            }

            if (_configService.RemoveFailedDownloads && trackedDownload.State == TrackedDownloadState.DownloadFailed)
            {
                try
                {
                    _logger.Debug("[{0}] Removing failed download from client", trackedDownload.DownloadItem.Title);
                    downloadClient.RemoveItem(trackedDownload.DownloadItem.DownloadClientId);

                    trackedDownload.State = TrackedDownloadState.Removed;
                }
                catch (NotSupportedException)
                {
                    UpdateStatusMessage(LogLevel.Debug, trackedDownload, "Removing item not supported by your download client.");
                }
            }
        }

        private bool FailedDownloadForRecentRelease(IDownloadClient downloadClient, TrackedDownload trackedDownload, List<History.History> matchingHistoryItems)
        {
            double ageHours;

            if (!Double.TryParse(matchingHistoryItems.First().Data.GetValueOrDefault("ageHours"), out ageHours))
            {
                _logger.Info("[{0}] Unable to determine age of failed download.", trackedDownload.DownloadItem.Title);
                return false;
            }

            if (ageHours > _configService.BlacklistGracePeriod)
            {
                _logger.Info("[{0}] Failed download is older than the grace period.", trackedDownload.DownloadItem.Title);
                return false;
            }

            if (trackedDownload.RetryCount >= _configService.BlacklistRetryLimit)
            {
                _logger.Info("[{0}] Retry limit reached.", trackedDownload.DownloadItem.Title);
                return false;
            }

            if (trackedDownload.RetryCount == 0 || trackedDownload.LastRetry.AddMinutes(_configService.BlacklistRetryInterval) < DateTime.UtcNow)
            {
                _logger.Info("[{0}] Retrying failed release.", trackedDownload.DownloadItem.Title);
                trackedDownload.LastRetry = DateTime.UtcNow;
                trackedDownload.RetryCount++;

                try
                {
                    downloadClient.RetryDownload(trackedDownload.DownloadItem.DownloadClientId);
                }
                catch (NotSupportedException ex)
                {
                    _logger.Debug("Retrying failed downloads is not supported by your download client");
                    return false;
                }
            }

            return true;
        }

        private List<History.History> GetHistoryItems(List<History.History> grabbedHistory, string downloadClientId)
        {
            return grabbedHistory.Where(h => downloadClientId.Equals(h.Data.GetValueOrDefault(DownloadTrackingService.DOWNLOAD_CLIENT_ID)))
                                 .ToList();
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
                DownloadClient = historyItem.Data.GetValueOrDefault(DownloadTrackingService.DOWNLOAD_CLIENT),
                DownloadClientId = historyItem.Data.GetValueOrDefault(DownloadTrackingService.DOWNLOAD_CLIENT_ID),
                Message = message
            };

            downloadFailedEvent.Data = downloadFailedEvent.Data.Merge(historyItem.Data);

            _eventAggregator.PublishEvent(downloadFailedEvent);
        }

        private void UpdateStatusMessage(LogLevel logLevel, TrackedDownload trackedDownload, String message, params object[] args)
        {
            var statusMessage = String.Format(message, args);
            var logMessage = String.Format("[{0}] {1}", trackedDownload.DownloadItem.Title, message);

            if (trackedDownload.StatusMessage != statusMessage)
            {
                trackedDownload.HasError = logLevel >= LogLevel.Warn;
                trackedDownload.StatusMessage = statusMessage;
                _logger.Log(logLevel, statusMessage);
            }
            else
            {
                _logger.Debug(logMessage);
            }
        }
    }
}
