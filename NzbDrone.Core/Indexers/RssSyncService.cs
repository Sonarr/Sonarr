using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.Indexers
{
    public interface IRssSyncService
    {
        void Sync();
    }

    public class RssSyncService : IRssSyncService, IExecute<RssSyncCommand>
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

            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);

            var qualifiedReports = decisions
                         .Where(c => c.Approved)
                         .Select(c => c.RemoteEpisode)
                         .OrderByDescending(c => c.ParsedEpisodeInfo.Quality)
                         .ThenBy(c => c.Episodes.Select(e => e.EpisodeNumber).MinOrDefault())
                         .ThenBy(c => c.Report.Age);

            var downloadedReports = new List<int>();

            foreach (var episodeParseResult in qualifiedReports)
            {
                try
                {
                    if (downloadedReports.Intersect(episodeParseResult.Episodes.Select(e => e.Id)).Any()) continue;

                    _downloadService.DownloadReport(episodeParseResult);
                    downloadedReports.AddRange(episodeParseResult.Episodes.Select(e => e.Id));
                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't add report to download queue. " + episodeParseResult, e);
                }
            }

            _logger.Info("RSS Sync Completed. Reports found: {0}, Fetches attempted: {1}", reports.Count, qualifiedReports.Count());
        }

        public void Execute(RssSyncCommand message)
        {
            Sync();
        }
    }
}