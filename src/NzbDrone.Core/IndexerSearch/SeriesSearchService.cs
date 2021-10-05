using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeriesSearchService : IExecute<SeriesSearchCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public SeriesSearchService(ISeriesService seriesService,
                                   ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _seriesService = seriesService;
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(SeriesSearchCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);

            var downloadedCount = 0;

            foreach (var season in series.Seasons.OrderBy(s => s.SeasonNumber))
            {
                if (!season.Monitored)
                {
                    _logger.Debug("Season {0} of {1} is not monitored, skipping search", season.SeasonNumber, series.Title);
                    continue;
                }

                var decisions = _releaseSearchService.SeasonSearch(message.SeriesId, season.SeasonNumber, false, true, message.Trigger == CommandTrigger.Manual, false);
                downloadedCount += _processDownloadDecisions.ProcessDecisions(decisions).Grabbed.Count;
            }

            _logger.ProgressInfo("Series search completed. {0} reports downloaded.", downloadedCount);
        }
    }
}
