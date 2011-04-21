using System.ServiceModel.Syndication;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public abstract class IndexerProviderBase
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly ConfigProvider _configProvider;
        protected readonly EpisodeProvider _episodeProvider;
        private readonly HttpProvider _httpProvider;
        protected readonly IRepository _repository;
        private readonly IndexerProvider _indexerProvider;
        protected readonly SeasonProvider _seasonProvider;
        protected readonly SeriesProvider _seriesProvider;


        public IndexerProviderBase(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
                                EpisodeProvider episodeProvider, ConfigProvider configProvider,
                                HttpProvider httpProvider, IRepository repository, IndexerProvider indexerProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _configProvider = configProvider;
            _httpProvider = httpProvider;
            _repository = repository;
            _indexerProvider = indexerProvider;
        }

        /// <summary>
        ///   Gets the name for the feed
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   Gets the source URL for the feed
        /// </summary>
        protected abstract string[] Urls { get; }


        protected IndexerSetting Settings
        {
            get
            {
                return _indexerProvider.GetSettings(GetType());
            }
        }

        /// <summary>
        ///   Fetches RSS feed and process each news item.
        /// </summary>
        public void Fetch()
        {
            Logger.Info("Fetching feeds from " + Settings.Name);

            foreach (var url in Urls)
            {
                Logger.Debug("Downloading RSS " + url);
                var feed = SyndicationFeed.Load(_httpProvider.DownloadXml(url)).Items;

                foreach (var item in feed)
                {
                    ProcessItem(item);
                }
            }

            Logger.Info("Finished processing feeds from " + Settings.Name);
        }

        private void ProcessItem(SyndicationItem feedItem)
        {
            Logger.Info("Processing RSS feed item " + feedItem.Title.Text);

            var parseResult = ParseFeed(feedItem);

            if (parseResult != null)
            {
                if (!_seriesProvider.IsMonitored(parseResult.SeriesId))
                {
                    Logger.Debug("{0} is present in the DB but not tracked. skipping.", parseResult.SeriesTitle);
                    return;
                }

                if (!_seriesProvider.QualityWanted(parseResult.SeriesId, parseResult.Quality))
                {
                    Logger.Debug("Post doesn't meet the quality requirements [{0}]. skipping.", parseResult.Quality);
                    return;
                }

                if (_seasonProvider.IsIgnored(parseResult.SeriesId, parseResult.SeasonNumber))
                {
                    Logger.Debug("Season {0} is currently set to ignore. skipping.", parseResult.SeasonNumber);
                    return;
                }

                if (!_episodeProvider.IsNeeded(parseResult))
                {
                    Logger.Debug("Episode {0} is not needed. skipping.", parseResult);
                    return;
                }

                //Should probably queue item to download
            }
        }

        /// <summary>
        ///   Parses the RSS feed item and.
        /// </summary>
        /// <param name = "item">RSS feed item to parse</param>
        /// <returns>Detailed episode info</returns>
        protected EpisodeParseResult ParseFeed(SyndicationItem item)
        {
            var episodeParseResult = Parser.ParseEpisodeInfo(item.Title.Text);
            if (episodeParseResult == null) return CustomParser(item, null);

            var seriesInfo = _seriesProvider.FindSeries(episodeParseResult.SeriesTitle);

            if (seriesInfo != null)
            {
                episodeParseResult.SeriesId = seriesInfo.SeriesId;
                episodeParseResult.SeriesTitle = seriesInfo.Title;
                return CustomParser(item, episodeParseResult);
            }

            Logger.Debug("Unable to map {0} to any of series in database", episodeParseResult.SeriesTitle);
            return CustomParser(item, episodeParseResult);
        }

        /// <summary>
        /// This method can be overwritten to provide indexer specific info parsing
        /// </summary>
        /// <param name="item">RSS item that needs to be parsed</param>
        /// <param name="currentResult">Result of the built in parse function.</param>
        /// <returns></returns>
        protected virtual EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            return currentResult;
        }

        /// <summary>
        ///   Generates direct link to download an NZB
        /// </summary>
        /// <param name = "item">RSS Feed item to generate the link for</param>
        /// <returns>Download link URL</returns>
        protected abstract string NzbDownloadUrl(SyndicationItem item);
    }
}