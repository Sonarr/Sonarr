using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchFeedFromIndexers
    {
        IList<ReportInfo> FetchRss(IIndexer indexer);

        IList<ReportInfo> Fetch(IIndexer indexer, SeasonSearchDefinition searchDefinition);
        IList<ReportInfo> Fetch(IIndexer indexer, SingleEpisodeSearchDefinition searchDefinition);
        IList<ReportInfo> Fetch(IIndexer indexer, PartialSeasonSearchDefinition searchDefinition);
        IList<ReportInfo> Fetch(IIndexer indexer, DailyEpisodeSearchDefinition searchDefinition);
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

        public IList<ReportInfo> Fetch(IIndexer indexer, SeasonSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetSeasonSearchUrls(searchDefinition.SceneTitle, searchDefinition.SeasonNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
            return result;
        }

        public IList<ReportInfo> Fetch(IIndexer indexer, SingleEpisodeSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetEpisodeSearchUrls(searchDefinition.SceneTitle, searchDefinition.SeasonNumber, searchDefinition.EpisodeNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
            return result;

        }

        public IList<ReportInfo> Fetch(IIndexer indexer, PartialSeasonSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetSeasonSearchUrls(searchDefinition.SceneTitle, searchDefinition.SeasonNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
            return result;
        }

        public IList<ReportInfo> Fetch(IIndexer indexer, DailyEpisodeSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetDailyEpisodeSearchUrls(searchDefinition.SceneTitle, searchDefinition.Airtime);
            var result = Fetch(indexer, searchUrls);

            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
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
                    if (webException.Message.Contains("503"))
                    {
                        _logger.Warn("{0} server is currently unavailable.{1} {2}", indexer.Name, url, webException.Message);
                    }
                    else
                    {
                        webException.Data.Add("FeedUrl", url);
                        _logger.ErrorException("An error occurred while processing feed. " + url, webException);
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