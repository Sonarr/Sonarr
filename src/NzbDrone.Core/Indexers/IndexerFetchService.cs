using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
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
    }

    public class FetchFeedService : IFetchFeedFromIndexers
    {
        private readonly Logger _logger;
        private readonly IHttpProvider _httpProvider;
        private readonly IIndexerParsingService _indexerParsingService;

        public FetchFeedService(IHttpProvider httpProvider, IIndexerParsingService indexerParsingService, Logger logger)
        {
            _httpProvider = httpProvider;
            _indexerParsingService = indexerParsingService;
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

            return result;
        }

        private IList<ReleaseInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria, int offset)
        {
            _logger.Debug("Searching for {0} offset: {1}", searchCriteria, offset);

            var searchUrls = indexer.GetSeasonSearchUrls(searchCriteria.QueryTitle, searchCriteria.Series.TvRageId, searchCriteria.SeasonNumber, offset);
            var result = Fetch(indexer, searchUrls);

            _logger.Info("{0} offset {1}. Found {2}", indexer, searchCriteria, result.Count);

            if (result.Count > 90 && 
                offset < 1000 &&
                indexer.SupportsPaging)
            {
                result.AddRange(Fetch(indexer, searchCriteria, offset + 100));
            }

            return result;
        }

        public IList<ReleaseInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetEpisodeSearchUrls(searchCriteria.QueryTitle, searchCriteria.Series.TvRageId, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer, searchCriteria, result.Count);
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
                        result.AddRange(_indexerParsingService.Parse(indexer, xml, url));
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
