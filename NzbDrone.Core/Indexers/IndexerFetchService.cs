using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using System.Linq;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchFeedFromIndexers
    {
        IList<ReportInfo> FetchRss(IIndexer indexer);

        IList<ReportInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria);
        IList<ReportInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria);
        IList<ReportInfo> Fetch(IIndexer indexer, DailyEpisodeSearchCriteria searchCriteria);
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


        public virtual IList<ReportInfo> FetchRss(IIndexer indexer)
        {
            _logger.Debug("Fetching feeds from " + indexer.Name);

            var result = Fetch(indexer, indexer.RecentFeed);

            _logger.Debug("Finished processing feeds from " + indexer.Name);

            return result;
        }

        public IList<ReportInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var result = Fetch(indexer, searchCriteria, 0).DistinctBy(c => c.NzbUrl).ToList();

            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer.Name, searchCriteria, result.Count);

            return result;
        }


        private IList<ReportInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria, int offset)
        {
            _logger.Debug("Searching for {0} offset: {1}", searchCriteria, offset);

            var searchUrls = indexer.GetSeasonSearchUrls(searchCriteria.QueryTitle, searchCriteria.SeriesTvRageId, searchCriteria.SeasonNumber, offset);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("{0} offset {1}. Found {2}", indexer.Name, searchCriteria, result.Count);

            if (result.Count > 90)
            {
                result.AddRange(Fetch(indexer, searchCriteria, offset + 90));
            }

            return result;
        }

        public IList<ReportInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetEpisodeSearchUrls(searchCriteria.QueryTitle, searchCriteria.SeriesTvRageId, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer.Name, searchCriteria, result.Count);
            return result;
        }

        public IList<ReportInfo> Fetch(IIndexer indexer, DailyEpisodeSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetDailyEpisodeSearchUrls(searchCriteria.QueryTitle, searchCriteria.SeriesTvRageId, searchCriteria.Airtime);
            var result = Fetch(indexer, searchUrls);

            _logger.Info("Finished searching {0} for {1}. Found {2}", indexer.Name, searchCriteria, result.Count);
            return result;
        }

        private List<ReportInfo> Fetch(IIndexer indexer, IEnumerable<string> urls)
        {
            var result = new List<ReportInfo>();

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
                    if (webException.Message.Contains("502") || webException.Message.Contains("503") || webException.Message.Contains("timed out"))
                    {
                        _logger.Warn("{0} server is currently unavailable. {1} {2}", indexer.Name, url, webException.Message);
                    }
                    else
                    {
                        _logger.Warn("{0} {1} {2}", indexer.Name, url, webException.Message);
                    }
                }
                catch (Exception feedEx)
                {
                    feedEx.Data.Add("FeedUrl", url);
                    _logger.ErrorException("An error occurred while processing feed. " + url, feedEx);
                }
            }

            result.ForEach(c => c.Indexer = indexer.Name);

            return result;
        }
    }
}