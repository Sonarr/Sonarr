using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchService : IExecute<SeasonSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReports _downloadApprovedReports;

        public SeasonSearchService(ISearchForNzb nzbSearchService, IDownloadApprovedReports downloadApprovedReports)
        {
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var decisions = _nzbSearchService.SeasonSearch(message.SeriesId, message.SeasonNumber);
            _downloadApprovedReports.DownloadApproved(decisions);
        }
    }
}
