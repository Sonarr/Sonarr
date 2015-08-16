using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadService
    {
        TrackedDownload Find(string downloadId);
        TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem);
    }

    public class TrackedDownloadService : ITrackedDownloadService
    {
        private readonly ICached<TrackedDownload> _cache;
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;
        private readonly IParsingService _parsingService;

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

            if (existingItem != null && existingItem.State != TrackedDownloadStage.Downloading)
            {
                existingItem.DownloadItem = downloadItem;
                return existingItem;
            }

            var trackedDownload = new TrackedDownload
            {
                DownloadClient = downloadClient.Id,
                DownloadItem = downloadItem,
                Protocol = downloadClient.Protocol
            };

            try
            {
                var parsedInfo = ParseTitleInfo(downloadItem);

                var historyItems = _historyService.FindByDownloadId(downloadItem.DownloadId);

                if (parsedInfo != null)
                {
                    trackedDownload.RemoteItem = _parsingService.Map(parsedInfo);
                }

                if (historyItems.Any())
                {


                    trackedDownload = MapWithHistory(parsedInfo, trackedDownload, historyItems);
                }

                if (trackedDownload.RemoteItem == null)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.DebugException("Failed to find episode for " + downloadItem.Title, e);
                return null;
            }

            _cache.Set(trackedDownload.DownloadItem.DownloadId, trackedDownload);
            return trackedDownload;
        }

        private ParsedInfo ParseTitleInfo(DownloadClientItem item)
        {
            switch (item.DownloadType)
            {
                case DownloadItemType.Series:
                    return Parser.Parser.ParseTitle(item.Title);
                case DownloadItemType.Movie:
                    return Parser.Parser.ParseMovieTitle(item.Title);
                default:
                    {
                        // If we can't obtain the serie, then return movie
                        var parsedInfo = Parser.Parser.ParseTitle(item.Title);

                        if (parsedInfo == null || _parsingService.GetSeries(parsedInfo.Title) == null)
                        {
                            return Parser.Parser.ParseMovieTitle(item.Title);
                        }

                        return parsedInfo;
                    }
            }
        }

        private TrackedDownload MapWithHistory(ParsedInfo parsedInfo, TrackedDownload trackedDownload, List<History.History> historyItems)
        {
            var firstHistoryItem = historyItems.OrderByDescending(h => h.Date).First();
            RemoteMovie remoteMovie = trackedDownload.RemoteItem as RemoteMovie;
            RemoteEpisode remoteEpisode = trackedDownload.RemoteItem as RemoteEpisode;

            if (parsedInfo == null ||
                (remoteMovie == null && remoteEpisode == null) ||
                (remoteMovie != null && remoteMovie.Movie == null) ||
                (remoteEpisode != null && remoteEpisode.Series == null) ||
                (remoteEpisode != null && remoteEpisode.Episodes.Empty()))
            {
                if (firstHistoryItem.SeriesId > 0)
                {
                    parsedInfo = Parser.Parser.ParseTitle(firstHistoryItem.SourceTitle);
                    if (parsedInfo != null)
                    {
                        trackedDownload.RemoteItem = _parsingService.Map(parsedInfo as ParsedEpisodeInfo, firstHistoryItem.SeriesId, historyItems.Where(v => v.EventType == HistoryEventType.Grabbed).Select(h => h.EpisodeId).Distinct());
                    }
                }
                else if (firstHistoryItem.MovieId > 0)
                {
                    parsedInfo = Parser.Parser.ParseMovieTitle(firstHistoryItem.SourceTitle);
                    if (parsedInfo != null)
                    {
                        trackedDownload.RemoteItem = _parsingService.Map(parsedInfo as ParsedMovieInfo, firstHistoryItem.MovieId);
                    }
                }
            }
            trackedDownload.State = GetStateFromHistory(firstHistoryItem.EventType);
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
    }
}
