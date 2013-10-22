using System;
using System.Linq;
using NLog;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public class FailedDownloadService : IExecute<FailedDownloadCommand>
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private static string DOWNLOAD_CLIENT = "downloadClient";
        private static string DOWNLOAD_CLIENT_ID = "downloadClientId";

        public FailedDownloadService(IProvideDownloadClient downloadClientProvider,
                                     IHistoryService historyService,
                                     IEventAggregator eventAggregator,
                                     Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void CheckForFailedDownloads()
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();
            var downloadClientHistory = downloadClient.GetHistory(0, 20).ToList();

            var failedItems = downloadClientHistory.Where(h => h.Status == HistoryStatus.Failed).ToList();

            if (!failedItems.Any())
            {
                _logger.Trace("Yay! No failed downloads");
                return;
            }

            var recentHistory = _historyService.BetweenDates(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, HistoryEventType.Grabbed);
            var failedHistory = _historyService.Failed();

            foreach (var failedItem in failedItems)
            {
                var failedLocal = failedItem;
                var historyItems = recentHistory.Where(h => h.Data.ContainsKey(DOWNLOAD_CLIENT) &&
                                                            h.Data[DOWNLOAD_CLIENT_ID].Equals(failedLocal.Id))
                                                .ToList();

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

                foreach (var historyItem in historyItems)
                {
                    _eventAggregator.PublishEvent(new DownloadFailedEvent
                    {
                        Series = historyItem.Series,
                        Episode = historyItem.Episode,
                        Quality = historyItem.Quality,
                        SourceTitle = historyItem.SourceTitle,
                        DownloadClient = historyItem.Data[DOWNLOAD_CLIENT],
                        DownloadClientId = historyItem.Data[DOWNLOAD_CLIENT_ID]
                    });
                }
            }
        }

        public void Execute(FailedDownloadCommand message)
        {
            CheckForFailedDownloads();
        }
    }
}
