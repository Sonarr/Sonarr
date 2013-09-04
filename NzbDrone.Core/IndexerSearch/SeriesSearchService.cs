using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;
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
        private readonly Logger _logger;

        public SeriesSearchService(ISeasonService seasonService,
                                   ISearchForNzb nzbSearchService,
                                   IDownloadApprovedReports downloadApprovedReports,
                                   Logger logger)
        {
            _seasonService = seasonService;
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
            _logger = logger;
        }

        public void Execute(SeriesSearchCommand message)
        {
            var seasons = _seasonService.GetSeasonsBySeries(message.SeriesId)
                                        .Where(s => s.SeasonNumber > 0)
                                        .OrderBy(s => s.SeasonNumber)
                                        .ToList();

            var downloadedCount = 0;

            foreach (var season in seasons)
            {
                var decisions = _nzbSearchService.SeasonSearch(message.SeriesId, season.SeasonNumber);
                downloadedCount += _downloadApprovedReports.DownloadApproved(decisions).Count;
            }

            _logger.Complete("Series search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
