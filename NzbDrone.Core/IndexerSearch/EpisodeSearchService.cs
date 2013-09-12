using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Messaging;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchService : IExecute<EpisodeSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReports _downloadApprovedReports;
        private readonly Logger _logger;

        public EpisodeSearchService(ISearchForNzb nzbSearchService,
                                    IDownloadApprovedReports downloadApprovedReports,
                                    Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
            _logger = logger;
        }

        public void Execute(EpisodeSearchCommand message)
        {
            var decisions = _nzbSearchService.EpisodeSearch(message.EpisodeId);
            var downloaded = _downloadApprovedReports.DownloadApproved(decisions);

            _logger.ProgressInfo("Episode search completed. {0} reports downloaded.", downloaded.Count);
        }
    }
}
