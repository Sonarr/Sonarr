using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{

    public interface ISyncRss
    {
        void Sync();
    }

    public class RssSyncService : ISyncRss
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IDownloadService _downloadService;
        private readonly Logger _logger;

        public RssSyncService(IFetchAndParseRss rssFetcherAndParser, IMakeDownloadDecision downloadDecisionMaker, IDownloadService downloadService, Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadService = downloadService;
            _logger = logger;
        }


        public void Sync()
        {
            _logger.Info("Starting RSS Sync");

            var parseResults = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(parseResults);

            var qualifiedReports = decisions
                         .Where(c => c.Approved)
                         .Select(c => c.Episode)
                         .OrderByDescending(c => c.Quality)
                         .ThenBy(c => c.Episodes.Select(e => e.EpisodeNumber).MinOrDefault())
                         .ThenBy(c => c.Report.Age);


            foreach (var episodeParseResult in qualifiedReports)
            {
                try
                {
                    _downloadService.DownloadReport(episodeParseResult);
                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't add report to download queue. " + episodeParseResult, e);
                }
            }

            _logger.Info("RSS Sync Completed. Reports found {0}, Fetches attempted {1}", parseResults.Count, qualifiedReports);
        }
    }


    public interface IFetchAndParseRss
    {
        List<ReportInfo> Fetch();
    }

    public class FetchAndParseRssService : IFetchAndParseRss
    {
        private readonly IIndexerService _indexerService;
        private readonly IFetchFeedFromIndexers _feedFetcher;

        public FetchAndParseRssService(IIndexerService indexerService, IFetchFeedFromIndexers feedFetcher)
        {
            _indexerService = indexerService;
            _feedFetcher = feedFetcher;
        }

        public List<ReportInfo> Fetch()
        {
            var result = new List<ReportInfo>();

            var indexers = _indexerService.GetAvailableIndexers();

            Parallel.ForEach(indexers, indexer =>
            {
                var indexerFeed = _feedFetcher.FetchRss(indexer);

                lock (result)
                {
                    result.AddRange(indexerFeed);
                }
            });

            return result;
        }
    }
}