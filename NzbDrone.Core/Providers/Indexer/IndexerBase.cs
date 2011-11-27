using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Web;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Search;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Indexer
{
    public abstract class IndexerBase
    {
        protected readonly Logger _logger;
        private readonly HttpProvider _httpProvider;
        protected readonly ConfigProvider _configProvider;

        private static readonly Regex TitleSearchRegex = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [Inject]
        protected IndexerBase(HttpProvider httpProvider, ConfigProvider configProvider)
        {
            _httpProvider = httpProvider;
            _configProvider = configProvider;

            _logger = LogManager.GetLogger(GetType().ToString());
        }

        public IndexerBase()
        {

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


        /// <summary>
        /// Gets the rss url for specific episode search
        /// </summary>
        /// <param name="searchModel">SearchModel containing episode information</param>
        /// <returns></returns>
        protected abstract IList<String> GetSearchUrls(SearchModel searchModel);

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
        ///   Fetches RSS feed and process each news item.
        /// </summary>
        public virtual IList<EpisodeParseResult> FetchRss()
        {
            _logger.Debug("Fetching feeds from " + Name);

            var result = new List<EpisodeParseResult>();

            foreach (var url in Urls)
            {
                result.AddRange(Fetch(url));
            }

            _logger.Info("Finished processing feeds from " + Name);
            return result;
        }

        public virtual IList<EpisodeParseResult> FetchSeason(string seriesTitle, int seasonNumber)
        {
            _logger.Debug("Searching {0} for {1}-Season {2}", Name, seriesTitle, seasonNumber);

            var result = new List<EpisodeParseResult>();

            var searchModel = new SearchModel
            {
                SeriesTitle = GetQueryTitle(seriesTitle),
                SeasonNumber = seasonNumber,
                SearchType = SearchType.SeasonSearch
            };

            var searchUrls = GetSearchUrls(searchModel);

            foreach (var url in searchUrls)
            {
                result.AddRange(Fetch(url));
            }

            result = result.Where(e => e.CleanTitle == Parser.NormalizeTitle(seriesTitle)).ToList();

            _logger.Info("Finished searching {0} for {1}-S{2}, Found {3}", Name, seriesTitle, seasonNumber, result.Count);
            return result;
        }

        public virtual IList<EpisodeParseResult> FetchPartialSeason(string seriesTitle, int seasonNumber, int episodePrefix)
        {
            _logger.Debug("Searching {0} for {1}-Season {2}, Prefix: {3}", Name, seriesTitle, seasonNumber, episodePrefix);

            var result = new List<EpisodeParseResult>();

            var searchModel = new SearchModel
            {
                SeriesTitle = GetQueryTitle(seriesTitle),
                SeasonNumber = seasonNumber,
                EpisodePrefix = episodePrefix,
                SearchType = SearchType.PartialSeasonSearch
            };

            var searchUrls = GetSearchUrls(searchModel);

            foreach (var url in searchUrls)
            {
                result.AddRange(Fetch(url));
            }

            result = result.Where(e => e.CleanTitle == Parser.NormalizeTitle(seriesTitle)).ToList();

            _logger.Info("Finished searching {0} for {1}-S{2}, Found {3}", Name, seriesTitle, seasonNumber, result.Count);
            return result;
        }

        public virtual IList<EpisodeParseResult> FetchEpisode(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            _logger.Debug("Searching {0} for {1}-S{2:00}E{3:00}", Name, seriesTitle, seasonNumber, episodeNumber);

            var result = new List<EpisodeParseResult>();

            var searchModel = new SearchModel
                                  {
                                      SeriesTitle = GetQueryTitle(seriesTitle),
                                      SeasonNumber = seasonNumber,
                                      EpisodeNumber = episodeNumber,
                                      SearchType = SearchType.EpisodeSearch
                                  };

            var searchUrls = GetSearchUrls(searchModel);

            foreach (var url in searchUrls)
            {
                result.AddRange(Fetch(url));
            }

            result = result.Where(e => e.CleanTitle == Parser.NormalizeTitle(seriesTitle)).ToList();

            _logger.Info("Finished searching {0} for {1}-S{2}E{3:00}, Found {4}", Name, seriesTitle, seasonNumber, episodeNumber, result.Count);
            return result;

        }

        public virtual IList<EpisodeParseResult> FetchDailyEpisode(string seriesTitle, DateTime airDate)
        {
            _logger.Debug("Searching {0} for {1}-{2}", Name, seriesTitle, airDate.ToShortDateString());

            var result = new List<EpisodeParseResult>();

            var searchModel = new SearchModel
            {
                SeriesTitle = GetQueryTitle(seriesTitle),
                AirDate = airDate,
                SearchType = SearchType.DailySearch
            };

            var searchUrls = GetSearchUrls(searchModel);

            foreach (var url in searchUrls)
            {
                result.AddRange(Fetch(url));
            }

            result = result.Where(e => e.CleanTitle == Parser.NormalizeTitle(seriesTitle)).ToList();

            _logger.Info("Finished searching {0} for {1}-{2}, Found {3}", Name, seriesTitle, airDate.ToShortDateString(), result.Count);
            return result;

        }

        private IEnumerable<EpisodeParseResult> Fetch(string url)
        {
            var result = new List<EpisodeParseResult>();

            try
            {
                _logger.Trace("Downloading RSS " + url);

                var reader = new SyndicationFeedXmlReader(_httpProvider.DownloadStream(url, Credentials));
                var feed = SyndicationFeed.Load(reader).Items;

                foreach (var item in feed)
                {
                    try
                    {
                        var parsedEpisode = ParseFeed(item);
                        if (parsedEpisode != null)
                        {
                            parsedEpisode.NzbUrl = NzbDownloadUrl(item);
                            parsedEpisode.Indexer = Name;
                            parsedEpisode.NzbTitle = item.Title.Text;
                            result.Add(parsedEpisode);
                        }
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

            return result;
        }

        /// <summary>
        ///   Parses the RSS feed item
        /// </summary>
        /// <param name = "item">RSS feed item to parse</param>
        /// <returns>Detailed episode info</returns>
        public EpisodeParseResult ParseFeed(SyndicationItem item)
        {
            var episodeParseResult = Parser.ParseTitle(item.Title.Text);

            return CustomParser(item, episodeParseResult);
        }

        public static string GetQueryTitle(string title)
        {
            var cleanTitle = TitleSearchRegex.Replace(title, "+").Trim('+', ' ');

            //remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{1,100}", "+");
            return cleanTitle;
        }
    }
}