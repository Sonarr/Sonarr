using System;
using System.ServiceModel.Syndication;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public abstract class IndexerProviderBase
    {
        protected readonly Logger _logger;
        protected readonly ConfigProvider _configProvider;
        protected readonly EpisodeProvider _episodeProvider;
        private readonly HttpProvider _httpProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly HistoryProvider _historyProvider;
        protected readonly SeasonProvider _seasonProvider;
        protected readonly SeriesProvider _seriesProvider;


        protected IndexerProviderBase(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
                                EpisodeProvider episodeProvider, ConfigProvider configProvider,
                                HttpProvider httpProvider, IndexerProvider indexerProvider, HistoryProvider historyProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _configProvider = configProvider;
            _httpProvider = httpProvider;
            _indexerProvider = indexerProvider;
            _historyProvider = historyProvider;
            _logger = LogManager.GetLogger(GetType().ToString());
        }

        /// <summary>
        ///   Gets the name for the feed
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   Gets the source URL for the feed
        /// </summary>
        protected abstract string[] Urls { get; }


        public IndexerSetting Settings
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
            _logger.Info("Fetching feeds from " + Settings.Name);

            foreach (var url in Urls)
            {
                try
                {
                    _logger.Debug("Downloading RSS " + url);
                    var feed = SyndicationFeed.Load(_httpProvider.DownloadXml(url)).Items;

                    foreach (var item in feed)
                    {
                        try
                        {
                            ProcessItem(item);
                        }
                        catch (Exception itemEx)
                        {
                            _logger.ErrorException("An error occurred while processing feed item", itemEx);
                        }

                    }
                }
                catch (Exception feedEx)
                {
                    _logger.ErrorException("An error occurred while processing feed", feedEx);
                }
            }

            _logger.Info("Finished processing feeds from " + Settings.Name);
        }

        internal void ProcessItem(SyndicationItem feedItem)
        {
            _logger.Debug("Processing RSS feed item " + feedItem.Title.Text);

            var parseResult = ParseFeed(feedItem);

            if (parseResult != null && parseResult.SeriesId != 0)
            {
                if (!_seriesProvider.IsMonitored(parseResult.SeriesId))
                {
                    _logger.Debug("{0} is present in the DB but not tracked. skipping.", parseResult.CleanTitle);
                    return;
                }

                if (!_seriesProvider.QualityWanted(parseResult.SeriesId, parseResult.Quality))
                {
                    _logger.Debug("Post doesn't meet the quality requirements [{0}]. skipping.", parseResult.Quality);
                    return;
                }

                if (_seasonProvider.IsIgnored(parseResult.SeriesId, parseResult.SeasonNumber))
                {
                    _logger.Debug("Season {0} is currently set to ignore. skipping.", parseResult.SeasonNumber);
                    return;
                }

                if (!_episodeProvider.IsNeeded(parseResult))
                {
                    _logger.Debug("Episode {0} is not needed. skipping.", parseResult);
                    return;
                }

                var episodes = _episodeProvider.GetEpisodeByParseResult(parseResult);

                foreach (var episode in episodes)
                {
                    if (_historyProvider.Exists(episode.EpisodeId, parseResult.Quality, parseResult.Proper))
                    {
                        _logger.Debug("Episode in history: {0}", episode.ToString());
                        continue;
                    }

                    //TODO: Add episode to sab

                    _historyProvider.Add(new History
                                                {
                                                    Date = DateTime.Now,
                                                    EpisodeId = episode.EpisodeId,
                                                    IsProper = parseResult.Proper,
                                                    NzbTitle = feedItem.Title.Text,
                                                    Quality = parseResult.Quality
                                                });
                }

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

            var seriesInfo = _seriesProvider.FindSeries(episodeParseResult.CleanTitle);

            if (seriesInfo != null)
            {
                episodeParseResult.SeriesId = seriesInfo.SeriesId;
                episodeParseResult.CleanTitle = seriesInfo.Title;
                return CustomParser(item, episodeParseResult);
            }

            _logger.Debug("Unable to map {0} to any of series in database", episodeParseResult.CleanTitle);
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