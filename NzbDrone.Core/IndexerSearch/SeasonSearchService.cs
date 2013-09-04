using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchService : IExecute<SeasonSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReports _downloadApprovedReports;
        private readonly Logger _logger;

        public SeasonSearchService(ISearchForNzb nzbSearchService,
                                   IDownloadApprovedReports downloadApprovedReports,
                                   Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
            _logger = logger;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var decisions = _nzbSearchService.SeasonSearch(message.SeriesId, message.SeasonNumber);
            var downloaded = _downloadApprovedReports.DownloadApproved(decisions);

            _logger.Complete("Season search completed. {0} reports downloaded.", downloaded.Count);
        }
    }
}
