using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Linq;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
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
        protected readonly SabProvider _sabProvider;
        protected readonly IEnumerable<ExternalNotificationProviderBase> _externalNotificationProvider;

        protected IndexerProviderBase(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
                                EpisodeProvider episodeProvider, ConfigProvider configProvider,
                                HttpProvider httpProvider, IndexerProvider indexerProvider,
                                HistoryProvider historyProvider, SabProvider sabProvider,
                                IEnumerable<ExternalNotificationProviderBase> externalNotificationProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _configProvider = configProvider;
            _httpProvider = httpProvider;
            _indexerProvider = indexerProvider;
            _historyProvider = historyProvider;
            _sabProvider = sabProvider;
            _externalNotificationProvider = externalNotificationProvider;
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

        /// <summary>
        /// Gets the credential.
        /// </summary>
        protected virtual NetworkCredential Credentials
        {
            get { return null; }
        }

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
        public List<Exception> Fetch()
        {
            _logger.Debug("Fetching feeds from " + Settings.Name);
            var exeptions = new List<Exception>();

            foreach (var url in Urls)
            {
                try
                {
                    _logger.Trace("Downloading RSS " + url);

                    var reader = new SyndicationFeedXmlReader(_httpProvider.DownloadStream(url, Credentials));
                    var feed = SyndicationFeed.Load(reader).Items;

                    foreach (var item in feed)
                    {
                        try
                        {
                            ProcessItem(item);
                        }
                        catch (Exception itemEx)
                        {
                            exeptions.Add(itemEx);
                            _logger.ErrorException("An error occurred while processing feed item", itemEx);
                        }

                    }
                }
                catch (Exception feedEx)
                {
                    exeptions.Add(feedEx);
                    _logger.ErrorException("An error occurred while processing feed", feedEx);
                }
            }

            _logger.Info("Finished processing feeds from " + Settings.Name);
            return exeptions;
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

                //Todo: How to handle full season files? Currently the episode list is completely empty for these releases
                //Todo: Should we assume that the release contains all the episodes that belong to this season and add them from the DB?

                if (!_episodeProvider.IsNeeded(parseResult))
                {
                    _logger.Debug("Episode {0} is not needed. skipping.", parseResult);
                    return;
                }

                var episodes = _episodeProvider.GetEpisodeByParseResult(parseResult);

                if (InHistory(episodes, parseResult, feedItem))
                {
                    return;
                }

                parseResult.EpisodeTitle = episodes[0].Title;
                var sabTitle = _sabProvider.GetSabTitle(parseResult);

                if (_configProvider.UseBlackhole)
                {
                    var blackholeDir = _configProvider.BlackholeDirectory;
                    var folder = !String.IsNullOrEmpty(blackholeDir) ? blackholeDir : Path.Combine(CentralDispatch.AppPath, "App_Data");
                    var fileName = Path.Combine(folder, sabTitle + ".nzb");
                    _logger.Info("Downloading NZB: {0}", sabTitle);
                    if (!_httpProvider.DownloadFile(NzbDownloadUrl(feedItem), fileName))
                    {
                        _logger.Info("Failed to download NZB");
                        return;
                    }
                }

                //else send to SAB
                else
                {
                    if (_sabProvider.IsInQueue(sabTitle))
                    {
                        return;
                    }

                    if (!_sabProvider.AddByUrl(NzbDownloadUrl(feedItem), sabTitle))
                    {
                        _logger.Warn("Unable to add item to SAB queue. {0} {1}", NzbDownloadUrl(feedItem), sabTitle);
                        return;
                    }
                }

                foreach (var episode in episodes)
                {
                    _historyProvider.Add(new History
                    {
                        Date = DateTime.Now,
                        EpisodeId = episode.EpisodeId,
                        IsProper = parseResult.Proper,
                        NzbTitle = feedItem.Title.Text,
                        Quality = parseResult.Quality,
                        Indexer = GetIndexerType()
                    });
                }

                //Notify!
                foreach (var notification in _externalNotificationProvider.Where(n => n.Settings.Enabled))
                {
                    notification.OnGrab(sabTitle);
                }
            }
        }

        /// <summary>
        ///   Parses the RSS feed item and.
        /// </summary>
        /// <param name = "item">RSS feed item to parse</param>
        /// <returns>Detailed episode info</returns>
        public EpisodeParseResult ParseFeed(SyndicationItem item)
        {
            var episodeParseResult = Parser.ParseEpisodeInfo(item.Title.Text);
            if (episodeParseResult == null) return null;

            var seriesInfo = _seriesProvider.FindSeries(episodeParseResult.CleanTitle);

            if (seriesInfo == null)
            {
                var seriesId = SceneNameHelper.FindByName(episodeParseResult.CleanTitle);

                if (seriesId != 0)
                    seriesInfo = _seriesProvider.GetSeries(seriesId);
            }

            if (seriesInfo != null)
            {
                episodeParseResult.SeriesId = seriesInfo.SeriesId;
                episodeParseResult.FolderName = new DirectoryInfo(seriesInfo.Path).Name; ;

                episodeParseResult.CleanTitle = seriesInfo.Title;
                return CustomParser(item, episodeParseResult);
            }

            _logger.Debug("Unable to map {0} to any of series in database", episodeParseResult.CleanTitle);
            return null;
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

        /// <summary>
        ///   Gets he IndexerType Enum for this indexer
        /// </summary>
        /// <returns>IndexerType Enum</returns>
        protected virtual IndexerType GetIndexerType()
        {
            return IndexerType.Unknown;
        }

        private bool InHistory(IList<Episode> episodes, EpisodeParseResult parseResult, SyndicationItem feedItem)
        {
            foreach (var episode in episodes)
            {
                if (_historyProvider.Exists(episode.EpisodeId, parseResult.Quality, parseResult.Proper))
                {
                    _logger.Debug("Episode in history: {0}", feedItem.Title.Text);
                    return true;
                }
            }
            return false;
        }
    }
}