using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchFeedFromIndexers
    {
        IList<ReportInfo> FetchRss(IIndexer indexer);

        IList<ReportInfo> Fetch(IIndexer indexer, SeasonSearchCriteria searchCriteria);
        IList<ReportInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria);
        IList<ReportInfo> Fetch(IIndexer indexer, PartialSeasonSearchCriteria searchCriteria);
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

            var searchUrls = indexer.GetSeasonSearchUrls(searchCriteria.QueryTitle, searchCriteria.SeriesRageTvId, searchCriteria.SeasonNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchCriteria, result.Count);
            return result;
        }

        public IList<ReportInfo> Fetch(IIndexer indexer, SingleEpisodeSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetEpisodeSearchUrls(searchCriteria.QueryTitle, searchCriteria.SeriesRageTvId, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchCriteria, result.Count);
            return result;

        }

        public IList<ReportInfo> Fetch(IIndexer indexer, PartialSeasonSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetSeasonSearchUrls(searchCriteria.QueryTitle, searchCriteria.SeriesRageTvId, searchCriteria.SeasonNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchCriteria, result.Count);
            return result;
        }

        public IList<ReportInfo> Fetch(IIndexer indexer, DailyEpisodeSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for {0}", searchCriteria);

            var searchUrls = indexer.GetDailyEpisodeSearchUrls(searchCriteria.QueryTitle, searchCriteria.SeriesRageTvId, searchCriteria.Airtime);
            var result = Fetch(indexer, searchUrls);

            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchCriteria, result.Count);
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
                    var stream = _httpProvider.DownloadStream(url);
                    result.AddRange(indexer.Parser.Process(stream));
                }
                catch (WebException webException)
                {
                    if (webException.Message.Contains("503") || webException.Message.Contains("timed out"))
                    {
                        _logger.Warn("{0} server is currently unavailable.{1} {2}", indexer.Name, url, webException.Message);
                    }
                    else
                    {
                        webException.Data.Add("FeedUrl", url);
                        _logger.WarnException("An error occurred while processing feed. " + url, webException);
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