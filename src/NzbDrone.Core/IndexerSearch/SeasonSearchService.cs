using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchService : IExecute<SeasonSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public SeasonSearchService(ISearchForNzb nzbSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions,
                                   Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        public void Execute(SeasonSearchCommand message)
        {
            var decisions = _nzbSearchService.SeasonSearch(message.SeriesId, message.SeasonNumber, false, message.Trigger == CommandTrigger.Manual, false);
            var processed = _processDownloadDecisions.ProcessDecisions(decisions);

            _logger.ProgressInfo("Season search completed. {0} reports downloaded.", processed.Grabbed.Count);
        }
    }
}
