using NLog;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedEpisodes : IHousekeepingTask
    {
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public CleanupOrphanedEpisodes(IEpisodeService episodeService, Logger logger)
        {
            _episodeService = episodeService;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Trace("Running orphaned episodes cleanup");
            _episodeService.CleanupOrphanedEpisodes();
        }
    }
}
