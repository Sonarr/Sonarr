using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using System.Linq;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchFeedFromIndexers
    {
        IList<ReleaseInfo> FetchRss(IIndexer indexer);
        IList<ReleaseInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(IIndexer indexer, DailyEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(IIndexer indexer, AnimeEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(IIndexer indexer, SpecialEpisodeSearchCriteria searchCriteria);
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

            _logger.Debug("Finished processing feeds from {0} found {1} releases", indexer, result.Count);

            return result;
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria)
        {
            return Fetch(indexer, searchCriteria, 0).DistinctBy(c => c.DownloadUrl).ToList();
        }

        private IList<ReleaseInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria, int offset)
        {
            var searchUrls = indexer.GetSeasonSearchUrls(searchCriteria.QueryTitles, searchCriteria.Series.TvRageId, searchCriteria.SeasonNumber, offset).ToList();

            if (searchUrls.Any())
            {
                _logger.Debug("Searching for {0} offset: {1}", searchCriteria, offset);

                var result = Fetch(indexer, searchUrls);

                _logger.Info("{0} offset {1}. Found {2}", indexer, offset, result.Count);

                if (indexer.SupportsPaging && result.Count >= indexer.SupportedPageSize && offset < 900)
                {
                    result.AddRange(Fetch(indexer, searchCriteria, offset + indexer.SupportedPageSize));
                }

                //Only log finish for the first call to this recursive method
                if (offset == 0)
                {
                    _logger.Info("Finished searching {0} for {1}. Found {2}", indexer, searchCriteria, result.Count);
                }

                return result;
            }

            return new List<ReleaseInfo>();
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria)
        {
            var searchUrls = indexer.GetEpisodeSearchUrls(searchCriteria.QueryTitles, searchCriteria.Series.TvRageId, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber).ToList();
            return Fetch(indexer, searchUrls, searchCriteria);
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, DailyEpisodeSearchCriteria searchCriteria)
        {
            var searchUrls = indexer.GetDailyEpisodeSearchUrls(searchCriteria.QueryTitles, searchCriteria.Series.TvRageId, searchCriteria.AirDate).ToList();
            return Fetch(indexer, searchUrls, searchCriteria);
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, AnimeEpisodeSearchCriteria searchCriteria)
        {
            var searchUrls = indexer.GetAnimeEpisodeSearchUrls(searchCriteria.SceneTitles, searchCriteria.Series.TvRageId, searchCriteria.AbsoluteEpisodeNumber).ToList();
            return Fetch(indexer, searchUrls, searchCriteria);
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, SpecialEpisodeSearchCriteria searchCriteria)
        {
            var searchUrls = new List<String>();

            foreach (var episodeQueryTitle in searchCriteria.EpisodeQueryTitles)
            {
                var urls = indexer.GetSearchUrls(episodeQueryTitle).ToList();

                if (urls.Any())
                {
                    _logger.Debug("Performing query of {0} for {1}", indexer, episodeQueryTitle);
                    searchUrls.AddRange(urls);
                }
            }

            return Fetch(indexer, searchUrls, searchCriteria);
        }

        private List<ReleaseInfo> Fetch(IIndexer indexer, IEnumerable<string> urls, SearchCriteriaBase searchCriteria)
        {
            var urlList = urls.ToList();

            if (urlList.Empty())
            {
                return new List<ReleaseInfo>();
            }

            _logger.Debug("Searching for {0}", searchCriteria);

            var result = Fetch(indexer, urlList);

            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer, searchCriteria, result.Count);

            return result;
        }

        private List<ReleaseInfo> Fetch(IIndexer indexer, IEnumerable<string> urls)
        {
            var result = new List<ReleaseInfo>();

            foreach (var url in urls)
            {
                try
                {
                    _logger.Debug("Downloading Feed " + url);
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

            result = result.DistinctBy(v => v.Guid).ToList();

            result.ForEach(c => 
            { 
                c.Indexer = indexer.Definition.Name;
                c.DownloadProtocol = indexer.Protocol; 
            });

            return result;
        }
    }
}
