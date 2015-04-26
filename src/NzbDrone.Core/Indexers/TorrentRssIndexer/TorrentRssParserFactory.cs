using System;
using NLog;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssParserFactory : ITorrentRssParserFactory
    {
        private static readonly ITorrentRssParserSettingsCache SettingsCache = new TorrentRssParserSettingsCache();

        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Get configured Parser based on <paramref name="settings"/>
        /// </summary>
        /// <param name="settings">Indexer Settings to use for Parser</param>
        /// <param name="fetchIndexerResponseFunc">Func to retrieve Feed</param>
        /// <returns>Configured Parser</returns>
        public TorrentRssParser GetParser(TorrentRssIndexerSettings settings, Func<IndexerRequest, IndexerResponse> fetchIndexerResponseFunc)
        {
            TorrentRssIndexerParserSettings parserSettings = SettingsCache.Get(settings.BaseUrl);

            if (parserSettings == null)
            {
                _logger.Debug("Parser Settings not in cache. Trying to Parse Feed.");
                
                var detector = new TorrentRssSettingsDetector();
                parserSettings = detector.Detect(settings, fetchIndexerResponseFunc);

                if (parserSettings == null)
                {
                    throw new Exception("Could not parse stream. See log for more information.");
                }

                SettingsCache.AddOrUpdate(settings.BaseUrl, parserSettings);
            }

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
