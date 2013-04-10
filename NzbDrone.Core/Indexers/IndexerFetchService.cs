using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchFeedFromIndexers
    {
        IList<IndexerParseResult> FetchRss(IIndexerBase indexer);

        IList<IndexerParseResult> Fetch(IIndexerBase indexer, SeasonSearchDefinition searchDefinition);
        IList<IndexerParseResult> Fetch(IIndexerBase indexer, SingleEpisodeSearchDefinition searchDefinition);
        IList<IndexerParseResult> Fetch(IIndexerBase indexer, PartialSeasonSearchDefinition searchDefinition);
        IList<IndexerParseResult> Fetch(IIndexerBase indexer, DailyEpisodeSearchDefinition searchDefinition);
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


        public virtual IList<IndexerParseResult> FetchRss(IIndexerBase indexer)
        {
            _logger.Debug("Fetching feeds from " + indexer.Name);

            var result = Fetch(indexer, indexer.RecentFeed);

            _logger.Debug("Finished processing feeds from " + indexer.Name);

            return result;
        }

        public IList<IndexerParseResult> Fetch(IIndexerBase indexer, SeasonSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetSeasonSearchUrls(searchDefinition.SceneTitle, searchDefinition.SeasonNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
            return result;
        }

        public IList<IndexerParseResult> Fetch(IIndexerBase indexer, SingleEpisodeSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetEpisodeSearchUrls(searchDefinition.SceneTitle, searchDefinition.SeasonNumber, searchDefinition.EpisodeNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
            return result;

        }

        public IList<IndexerParseResult> Fetch(IIndexerBase indexer, PartialSeasonSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetSeasonSearchUrls(searchDefinition.SceneTitle, searchDefinition.SeasonNumber);
            var result = Fetch(indexer, searchUrls);


            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
            return result;
        }

        public IList<IndexerParseResult> Fetch(IIndexerBase indexer, DailyEpisodeSearchDefinition searchDefinition)
        {
            _logger.Debug("Searching for {0}", searchDefinition);

            var searchUrls = indexer.GetDailyEpisodeSearchUrls(searchDefinition.SceneTitle, searchDefinition.Airtime);
            var result = Fetch(indexer, searchUrls);

            _logger.Info("Finished searching {0} on {1}. Found {2}", indexer.Name, searchDefinition, result.Count);
            return result;
        }

        private List<IndexerParseResult> Fetch(IIndexerBase indexer, IEnumerable<string> urls)
        {
            var result = new List<IndexerParseResult>();

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