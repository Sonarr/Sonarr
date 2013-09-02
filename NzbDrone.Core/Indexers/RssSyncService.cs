using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;
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
        private readonly IDownloadApprovedReports _downloadApprovedReports;
        private readonly Logger _logger;

        public RssSyncService(IFetchAndParseRss rssFetcherAndParser,
                              IMakeDownloadDecision downloadDecisionMaker,
                              IDownloadApprovedReports downloadApprovedReports,
                              Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadApprovedReports = downloadApprovedReports;
            _logger = logger;
        }


        public void Sync()
        {
            _logger.Info("Starting RSS Sync");

            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var qualifiedReports = _downloadApprovedReports.DownloadApproved(decisions);

            _logger.Complete("RSS Sync Completed. Reports found: {0}, Reports downloaded: {1}", reports.Count, qualifiedReports.Count());
        }

        public void Execute(RssSyncCommand message)
        {
            Sync();
        }
    }
}