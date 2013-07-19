using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchService : IExecute<EpisodeSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReports _downloadApprovedReports;

        public EpisodeSearchService(ISearchForNzb nzbSearchService, IDownloadApprovedReports downloadApprovedReports)
        {
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
        }

        public void Execute(EpisodeSearchCommand message)
        {
            var decisions = _nzbSearchService.EpisodeSearch(message.EpisodeId);
            _downloadApprovedReports.DownloadApproved(decisions);
        }
    }
}
