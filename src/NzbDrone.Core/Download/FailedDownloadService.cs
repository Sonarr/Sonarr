using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public interface IFailedDownloadService
    {
        void MarkAsFailed(int historyId);
    }

    public class FailedDownloadService : IFailedDownloadService, IExecute<FailedDownloadCommand>
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        private readonly IDownloadClient _downloadClient;

        private static string DOWNLOAD_CLIENT = "downloadClient";
        private static string DOWNLOAD_CLIENT_ID = "downloadClientId";

        public FailedDownloadService(IProvideDownloadClient downloadClientProvider,
                                     IHistoryService historyService,
                                     IEventAggregator eventAggregator,
                                     IConfigService configService,
                                     Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _logger = logger;

            _downloadClient = _downloadClientProvider.GetDownloadClient();
        }

        public void MarkAsFailed(int historyId)
        {
            var item = _historyService.Get(historyId);
            PublishDownloadFailedEvent(new List<History.History> {item}, "Manually marked as failed");
        }

        private void CheckForFailedDownloads()
        {
            if (!_configService.EnableFailedDownloadHandling)
            {
                _logger.Trace("Failed Download Handling is not enabled");
                return;
            }

            var grabbedHistory = _historyService.Grabbed();
            var failedHistory = _historyService.Failed();

            CheckQueue(grabbedHistory, failedHistory);
            CheckHistory(grabbedHistory, failedHistory);
        }

        private void CheckQueue(List<History.History> grabbedHistory, List<History.History> failedHistory)
        {
            var downloadClientQueue = _downloadClient.GetQueue().ToList();
            var failedItems = downloadClientQueue.Where(q => q.Title.StartsWith("ENCRYPTED / ")).ToList();

            if (!failedItems.Any())
            {
                _logger.Trace("Yay! No encrypted downloads");
                return;
            }

            foreach (var failedItem in failedItems)
            {
                var failedLocal = failedItem;
                var historyItems = GetHistoryItems(grabbedHistory, failedLocal.Id);

                if (!historyItems.Any())
                {
                    _logger.Trace("Unable to find matching history item");
                    continue;
                }

                if (failedHistory.Any(h => h.Data.ContainsKey(DOWNLOAD_CLIENT_ID) &&
                                           h.Data[DOWNLOAD_CLIENT_ID].Equals(failedLocal.Id)))
                {
                    _logger.Trace("Already added to history as failed");
                    continue;
                }

                PublishDownloadFailedEvent(historyItems, "Encypted download detected");

                if (_configService.RemoveFailedDownloads)
                {
                    _logger.Info("Removing encrypted download from queue: {0}", failedItem.Title.Replace("ENCRYPTED / ", ""));
                    _downloadClient.RemoveFromQueue(failedItem.Id);
                }
            }
        }

        private void CheckHistory(List<History.History> grabbedHistory, List<History.History> failedHistory)
        {
            var downloadClientHistory = _downloadClient.GetHistory(0, 20).ToList();
            var failedItems = downloadClientHistory.Where(h => h.Status == HistoryStatus.Failed).ToList();

            if (!failedItems.Any())
            {
                _logger.Trace("Yay! No failed downloads");
                return;
            }

            foreach (var failedItem in failedItems)
            {
                var failedLocal = failedItem;
                var historyItems = GetHistoryItems(grabbedHistory, failedLocal.Id);

                if (!historyItems.Any())
                {
                    _logger.Trace("Unable to find matching history item");
                    continue;
                }

                if (failedHistory.Any(h => h.Data.ContainsKey(DOWNLOAD_CLIENT_ID) &&
                                           h.Data[DOWNLOAD_CLIENT_ID].Equals(failedLocal.Id)))
                {
                    _logger.Trace("Already added to history as failed");
                    continue;
                }

                PublishDownloadFailedEvent(historyItems, failedItem.Message);

                if (_configService.RemoveFailedDownloads)
                {
                    _logger.Info("Removing failed download from history: {0}", failedItem.Title);
                    _downloadClient.RemoveFromHistory(failedItem.Id);
                }
            }
        }

        private List<History.History> GetHistoryItems(List<History.History> grabbedHistory, string downloadClientId)
        {
            return grabbedHistory.Where(h => h.Data.ContainsKey(DOWNLOAD_CLIENT) &&
                                             h.Data[DOWNLOAD_CLIENT_ID].Equals(downloadClientId))
                                 .ToList();
        }

        private void PublishDownloadFailedEvent(List<History.History> historyItems, string message)
        {
            var historyItem = historyItems.First();
            _eventAggregator.PublishEvent(new DownloadFailedEvent
            {
                SeriesId = historyItem.SeriesId,
                EpisodeIds = historyItems.Select(h => h.EpisodeId).ToList(),
                Quality = historyItem.Quality,
                SourceTitle = historyItem.SourceTitle,
                DownloadClient = historyItem.Data[DOWNLOAD_CLIENT],
                DownloadClientId = historyItem.Data[DOWNLOAD_CLIENT_ID],
                Message = message
            });
        }

        public void Execute(FailedDownloadCommand message)
        {
            CheckForFailedDownloads();
        }
    }
}
