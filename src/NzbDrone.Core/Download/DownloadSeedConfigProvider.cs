using System;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadSeedConfigProvider
    {
        TorrentSeedConfiguration GetSeedConfiguration(string infoHash);
    }

    public class DownloadSeedConfigProvider : IDownloadSeedConfigProvider
    {
        private readonly Logger _logger;
        private readonly ISeedConfigProvider _indexerSeedConfigProvider;
        private readonly IDownloadHistoryService _downloadHistoryService;

        private class CachedSeedConfiguration
        {
            public int IndexerId { get; set; }
            public bool FullSeason { get; set; }
        }

        private readonly ICached<CachedSeedConfiguration> _cacheDownloads;

        public DownloadSeedConfigProvider(IDownloadHistoryService downloadHistoryService, ISeedConfigProvider indexerSeedConfigProvider, ICacheManager cacheManager, Logger logger)
        {
            _logger = logger;
            _indexerSeedConfigProvider = indexerSeedConfigProvider;
            _downloadHistoryService = downloadHistoryService;

            _cacheDownloads = cacheManager.GetRollingCache<CachedSeedConfiguration>(GetType(), "indexerByHash", TimeSpan.FromHours(1));
        }

        public TorrentSeedConfiguration GetSeedConfiguration(string infoHash)
        {
            if (infoHash.IsNullOrWhiteSpace())
            {
                return null;
            }

            infoHash = infoHash.ToUpper();

            var cachedConfig = _cacheDownloads.Get(infoHash, () => FetchIndexer(infoHash));

            if (cachedConfig == null)
            {
                return null;
            }

            var seedConfig = _indexerSeedConfigProvider.GetSeedConfiguration(cachedConfig.IndexerId, cachedConfig.FullSeason);

            return seedConfig;
        }

        private CachedSeedConfiguration FetchIndexer(string infoHash)
        {
            var historyItem = _downloadHistoryService.GetLatestGrab(infoHash);

            if (historyItem == null)
            {
                _logger.Debug("No download history item for infohash {0}, unable to provide seed configuration", infoHash);
                return null;
            }

            ParsedEpisodeInfo parsedEpisodeInfo = null;
            if (historyItem.Release != null)
            {
                parsedEpisodeInfo = Parser.Parser.ParseTitle(historyItem.Release.Title);
            }

            if (parsedEpisodeInfo == null)
            {
                _logger.Debug("No parsed title in download history item for infohash {0}, unable to provide seed configuration", infoHash);
                return null;
            }

            return new CachedSeedConfiguration
            {
                IndexerId = historyItem.IndexerId,
                FullSeason = parsedEpisodeInfo.FullSeason
            };
        }
    }
}
