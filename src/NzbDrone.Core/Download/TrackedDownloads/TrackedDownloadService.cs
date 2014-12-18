using System;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadService
    {
        TrackedDownload Find(string downloadId);
        TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem);
    }

    public class TrackedDownloadService : ITrackedDownloadService,
                                          IHandle<SceneMappingsUpdatedEvent>
    {
        private readonly IParsingService _parsingService;
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;
        private readonly ICached<TrackedDownload> _cache;

        public TrackedDownloadService(IParsingService parsingService,
            ICacheManager cacheManager,
            IHistoryService historyService,
            Logger logger)
        {
            _parsingService = parsingService;
            _historyService = historyService;
            _cache = cacheManager.GetCache<TrackedDownload>(GetType());
            _logger = logger;
        }

        public TrackedDownload Find(string downloadId)
        {
            return _cache.Find(downloadId);
        }

        public TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem)
        {
            var existingItem = Find(downloadItem.DownloadId);

            if (existingItem != null)
            {
                existingItem.DownloadItem = downloadItem;
                return existingItem;
            }

            var trackedDownload = new TrackedDownload
            {
                TrackingId = downloadClient.Id + "-" + downloadItem.DownloadId,
                DownloadClient = downloadClient.Id,
                DownloadItem = downloadItem,
                Protocol = downloadClient.Protocol
            };

            var historyItem = _historyService.MostRecentForDownloadId(downloadItem.DownloadId);
            if (historyItem != null)
            {
                trackedDownload.State = GetStateFromHistory(historyItem.EventType);
            }

            try
            {
                var parsedEpisodeInfo = Parser.Parser.ParseTitle(trackedDownload.DownloadItem.Title);
                if (parsedEpisodeInfo == null) return null;

                var remoteEpisode = _parsingService.Map(parsedEpisodeInfo);
                if (remoteEpisode.Series == null)
                {
                    return null;
                }

                trackedDownload.RemoteEpisode = remoteEpisode;
            }
            catch (Exception e)
            {
                _logger.DebugException("Failed to find episode for " + downloadItem.Title, e);
                return null;
            }

            if (trackedDownload.State != TrackedDownloadStage.Downloading)
            {
                _cache.Set(downloadItem.DownloadId, trackedDownload);
            }

            return trackedDownload;
        }

        private static TrackedDownloadStage GetStateFromHistory(HistoryEventType eventType)
        {
            switch (eventType)
            {
                case HistoryEventType.DownloadFolderImported:
                    return TrackedDownloadStage.Imported;
                case HistoryEventType.DownloadFailed:
                    return TrackedDownloadStage.DownloadFailed;
                default:
                    return TrackedDownloadStage.Downloading;
            }
        }

        public void Handle(SceneMappingsUpdatedEvent message)
        {
            _cache.Clear();
        }
    }
}