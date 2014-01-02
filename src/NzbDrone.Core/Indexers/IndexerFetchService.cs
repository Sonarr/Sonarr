using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using System.Linq;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchFeedFromIndexers
    {
        IList<ReleaseInfo> FetchRss(IIndexer indexer);

        IList<ReleaseInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(IIndexer indexer, DailyEpisodeSearchCriteria searchCriteria);
    }

    public class FetchFeedService : IFetchFeedFromIndexers
    {
        private readonly Logger _logger;
        private readonly IHttpProvider _httpProvider;


        public FetchFeedService(IHttpProvider httpProvider, Logger logger)
        {
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public virtual IList<ReleaseInfo> FetchRss(IIndexer indexer)
        {
            _logger.Debug("Fetching feeds from " + indexer);

            var result = Fetch(indexer, indexer.RecentFeed);

            _logger.Debug("Finished processing feeds from " + indexer);

            return result;
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var result = Fetch(indexer, searchCriteria, 0).DistinctBy(c => c.DownloadUrl).ToList();

            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer, searchCriteria, result.Count);

            if (result.Count == 0 && searchCriteria.SeasonNumber == 0)
            {
                // no results? fetch season special episodes using query
                return FetchEpisodesUsingQuery(indexer, searchCriteria);
            }

            return result;
        }

        private IList<ReleaseInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria, int offset)
        {
            _logger.Debug("Searching for {0} offset: {1}", searchCriteria, offset);

            var searchUrls = indexer.GetSeasonSearchUrls(searchCriteria.QueryTitle, searchCriteria.Series.TvRageId, searchCriteria.SeasonNumber, offset);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("{0} offset {1}. Found {2}", indexer, searchCriteria, result.Count);

            if (result.Count > 90)
            {
                result.AddRange(Fetch(indexer, searchCriteria, offset + 90));
            }

            return result;
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetEpisodeSearchUrls(searchCriteria.QueryTitle, searchCriteria.Series.TvRageId, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber);
            var result = Fetch(indexer, searchUrls);
            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer, searchCriteria, result.Count);

            if (result.Count == 0)
            {
                // no results? use search query with episode titles as fallback
                return FetchEpisodesUsingQuery(indexer, searchCriteria);
            }
            return result;
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, DailyEpisodeSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetDailyEpisodeSearchUrls(searchCriteria.QueryTitle, searchCriteria.Series.TvRageId, searchCriteria.AirDate);
            var result = Fetch(indexer, searchUrls);

            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer, searchCriteria, result.Count);
            return result;
        }

        private IList<ReleaseInfo> FetchEpisodesUsingQuery(IIndexer indexer, SearchCriteriaBase searchCriteria)
        {
            var queryUrls = new List<string>();
            foreach (var episode in searchCriteria.Episodes)
            {
                if (!string.IsNullOrEmpty(episode.Title))
                {
                    // build query string for "<series> <episode-title>"
                    string episodeTitle = Regex.Replace(episode.Title, @"\W+", "+");
                    string query = searchCriteria.QueryTitle + "+" + episodeTitle;
                    _logger.Debug("Performing query of {0} for {1}", indexer, query);
                    queryUrls.AddRange(indexer.GetSearchUrls(query, 0, 1000));
                }

                if (episode.SeasonNumber != 0)
                {
                    // build query string for "<series> S03E08"
                    string query = searchCriteria.QueryTitle + "+" + string.Format("S{0:00}E{1:00}", episode.SeasonNumber, episode.EpisodeNumber);
                    _logger.Debug("Performing query of {0} for {1}", indexer, query);
                    queryUrls.AddRange(indexer.GetSearchUrls(query, 0, 1000));
                }
            }
            var result = Fetch(indexer, queryUrls);
            if (queryUrls.Count > 0)
            {
                _logger.Info("Finished searching {0} for {1} using query strings. Found {2}", indexer, searchCriteria, result.Count);
            }
            return result;
        }

        private List<ReleaseInfo> Fetch(IIndexer indexer, IEnumerable<string> urls)
        {
            var result = new List<ReleaseInfo>();

            foreach (var url in urls)
            {
                try
                {
                    _logger.Trace("Downloading Feed " + url);
                    var xml = _httpProvider.DownloadString(url);
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        result.AddRange(indexer.Parser.Process(xml, url));
                    }
                    else
                    {
                        _logger.Warn("{0} returned empty response.", url);
                    }

                }
                catch (WebException webException)
                {
                    if (webException.Message.Contains("502") || webException.Message.Contains("503") ||
                        webException.Message.Contains("timed out"))
                    {
                        _logger.Warn("{0} server is currently unavailable. {1} {2}", indexer, url, webException.Message);
                    }
                    else
                    {
                        _logger.Warn("{0} {1} {2}", indexer, url, webException.Message);
                    }
                }
                catch (ApiKeyException)
                {
                    _logger.Warn("Invalid API Key for {0} {1}", indexer, url);
                }
                catch (Exception feedEx)
                {
                    feedEx.Data.Add("FeedUrl", url);
                    _logger.ErrorException("An error occurred while processing feed. " + url, feedEx);
                }
            }

            result.ForEach(c => c.Indexer = indexer.Definition.Name);

            return result;
        }
    }
}
