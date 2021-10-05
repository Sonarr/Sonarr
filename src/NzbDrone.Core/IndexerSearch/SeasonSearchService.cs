using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchService : IExecute<SeasonSearchCommand>
    {
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public SeasonSearchService(ISearchForReleases releaseSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var decisions = _releaseSearchService.SeasonSearch(message.SeriesId, message.SeasonNumber, false, true, message.Trigger == CommandTrigger.Manual, false);
            var processed = _processDownloadDecisions.ProcessDecisions(decisions);

            _logger.ProgressInfo("Season search completed. {0} reports downloaded.", processed.Grabbed.Count);
        }
    }
}
