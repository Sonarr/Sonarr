using System;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.Indexers
{

    public interface IRssSyncService
    {
        void Sync();
    }

    public class RssSyncService : IRssSyncService
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

            //TODO: this will download multiple of same episode if they show up in RSS. need to
            
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

            _logger.Info("RSS Sync Completed. Reports found: {0}, Fetches attempted: {1}", parseResults.Count, qualifiedReports.Count());
        }
    }
}