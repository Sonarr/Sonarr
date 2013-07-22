using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeriesSearchService : IExecute<SeriesSearchCommand>
    {
        private readonly ISeasonService _seasonService;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReports _downloadApprovedReports;

        public SeriesSearchService(ISeasonService seasonService,
                                   ISearchForNzb nzbSearchService,
                                   IDownloadApprovedReports downloadApprovedReports)
        {
            _seasonService = seasonService;
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
        }

        public void Execute(SeriesSearchCommand message)
        {
            var seasons = _seasonService.GetSeasonsBySeries(message.SeriesId)
                                        .Where(s => s.SeasonNumber > 0)
                                        .OrderBy(s => s.SeasonNumber)
                                        .ToList();

            foreach (var season in seasons)
            {
                var decisions = _nzbSearchService.SeasonSearch(message.SeriesId, season.SeasonNumber);
                _downloadApprovedReports.DownloadApproved(decisions);
            }
        }
    }
}
