using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeriesSearchService : IExecute<SeriesSearchCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReports _downloadApprovedReports;
        private readonly Logger _logger;

        public SeriesSearchService(ISeriesService seriesService,
                                   ISearchForNzb nzbSearchService,
                                   IDownloadApprovedReports downloadApprovedReports,
                                   Logger logger)
        {
            _seriesService = seriesService;
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
            _logger = logger;
        }

        public void Execute(SeriesSearchCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);

            var downloadedCount = 0;

            foreach (var season in series.Seasons)
            {
                var decisions = _nzbSearchService.SeasonSearch(message.SeriesId, season.SeasonNumber);
                downloadedCount += _downloadApprovedReports.DownloadApproved(decisions).Count;
            }

            _logger.ProgressInfo("Series search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
