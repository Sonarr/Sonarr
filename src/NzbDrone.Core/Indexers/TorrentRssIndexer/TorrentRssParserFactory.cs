using System;
using NLog;
using NzbDrone.Common.Cache;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssParserFactory : ITorrentRssParserFactory
    {
        protected readonly Logger _logger;
        
        private readonly ICached<TorrentRssIndexerParserSettings> _settingsCache;

        private readonly ITorrentRssSettingsDetector _torrentRssSettingsDetector;

        public TorrentRssParserFactory(ICacheManager cacheManager, ITorrentRssSettingsDetector torrentRssSettingsDetector, Logger logger)
        {
            _settingsCache = cacheManager.GetCache<TorrentRssIndexerParserSettings>(GetType());
            _torrentRssSettingsDetector = torrentRssSettingsDetector;
            _logger = logger;
        }

        /// <summary>
        /// Get configured Parser based on <paramref name="settings"/>
        /// </summary>
        /// <param name="settings">Indexer Settings to use for Parser</param>
        /// <returns>Configured Parser</returns>
        public TorrentRssParser GetParser(TorrentRssIndexerSettings settings)
        {
            TorrentRssIndexerParserSettings parserSettings = _settingsCache.Get(settings.BaseUrl,
                () =>
                {
                    _logger.Debug("Parser Settings not in cache. Trying to parse feed {0}", settings.BaseUrl);
                    var parserSettingsToStore = _torrentRssSettingsDetector.Detect(settings);

                    if (parserSettingsToStore == null)
                    {
                        throw new Exception(string.Format("Could not parse feed from {0}", settings.BaseUrl));
                    }

                    return parserSettingsToStore;
                },
                    new TimeSpan(7, 0, 0, 0));

            if (parserSettings.UseEZTVFormat)
            {
                return new EzrssTorrentRssParser();
            }
            else
            {
                return new TorrentRssParser { UseGuidInfoUrl = false, ParseSeedersInDescription = parserSettings.ParseSeedersInDescription, ParseSizeInDescription = parserSettings.ParseSizeInDescription, SizeElementName = parserSettings.SizeElementName };
            }
        }
    }
}
