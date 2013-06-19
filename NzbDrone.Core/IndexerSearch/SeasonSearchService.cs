using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchService : IExecute<SeasonSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReportsService _downloadApprovedReportsService;

        public SeasonSearchService(ISearchForNzb nzbSearchService, IDownloadApprovedReportsService downloadApprovedReportsService)
        {
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReportsService = downloadApprovedReportsService;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var decisions = _nzbSearchService.SeasonSearch(message.SeriesId, message.SeasonNumber);
            var qualified = _downloadApprovedReportsService.DownloadApproved(decisions);
        }
    }
}
