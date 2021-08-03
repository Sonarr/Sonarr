using System;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.Exceptions;

namespace NzbDrone.Core.Indexers.TorrentRss
{
    public interface ITorrentRssParserFactory
    {
        TorrentRssParser GetParser(TorrentRssIndexerSettings settings);
    }

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

        public TorrentRssParser GetParser(TorrentRssIndexerSettings indexerSettings)
        {
            var key = indexerSettings.ToJson();
            var parserSettings = _settingsCache.Get(key, () => DetectParserSettings(indexerSettings), TimeSpan.FromDays(7));

            if (parserSettings.UseEZTVFormat)
            {
                return new EzrssTorrentRssParser();
            }
            else
            {
                return new TorrentRssParser
                {
                    UseGuidInfoUrl = false,
                    ParseSeedersInDescription = parserSettings.ParseSeedersInDescription,

                    UseEnclosureUrl = parserSettings.UseEnclosureUrl,
                    UseEnclosureLength = parserSettings.UseEnclosureLength,
                    ParseSizeInDescription = parserSettings.ParseSizeInDescription,
                    SizeElementName = parserSettings.SizeElementName
                };
            }
        }

        private TorrentRssIndexerParserSettings DetectParserSettings(TorrentRssIndexerSettings indexerSettings)
        {
            var settings = _torrentRssSettingsDetector.Detect(indexerSettings);

            if (settings == null)
            {
                throw new UnsupportedFeedException("Could not parse feed from {0}", indexerSettings.BaseUrl);
            }

            return settings;
        }
    }
}
