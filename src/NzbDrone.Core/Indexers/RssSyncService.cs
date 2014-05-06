using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Indexers
{
    public interface IRssSyncService
    {
        List<DownloadDecision> Sync();
    }

    public class RssSyncService : IRssSyncService, IExecute<RssSyncCommand>
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IDownloadApprovedReports _downloadApprovedReports;
        private readonly IEpisodeSearchService _episodeSearchService;
        private readonly Logger _logger;

        public RssSyncService(IFetchAndParseRss rssFetcherAndParser,
                              IMakeDownloadDecision downloadDecisionMaker,
                              IDownloadApprovedReports downloadApprovedReports,
                              IEpisodeSearchService episodeSearchService,
                              Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadApprovedReports = downloadApprovedReports;
            _episodeSearchService = episodeSearchService;
            _logger = logger;
        }


        public List<DownloadDecision> Sync()
        {
            _logger.ProgressInfo("Starting RSS Sync");

            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var downloaded = _downloadApprovedReports.DownloadApproved(decisions);

            _logger.ProgressInfo("RSS Sync Completed. Reports found: {0}, Reports downloaded: {1}", reports.Count, downloaded.Count());

            return downloaded;
        }

        public void Execute(RssSyncCommand message)
        {
            var downloaded = Sync();

            if (message.LastExecutionTime.HasValue && DateTime.UtcNow.Subtract(message.LastExecutionTime.Value).TotalHours > 3)
            {
                _logger.Info("RSS Sync hasn't run since: {0}. Searching for any missing episodes since then.", message.LastExecutionTime.Value);
                _episodeSearchService.MissingEpisodesAiredAfter(message.LastExecutionTime.Value.AddDays(-1), downloaded.SelectMany(d => d.RemoteEpisode.Episodes).Select(e => e.Id));
            }
        }
    }
}
