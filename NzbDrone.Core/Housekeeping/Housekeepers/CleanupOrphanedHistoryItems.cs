using NLog;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedHistoryItems : IHousekeepingTask
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public CleanupOrphanedHistoryItems(IHistoryService historyService, Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Trace("Running orphaned history cleanup");
            _historyService.CleanupOrphaned();
        }
    }
}
