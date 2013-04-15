using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MonitoredEpisodeSpecification : IDecisionEngineSpecification
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesRepository _seriesRepository;
        private readonly Logger _logger;

        public MonitoredEpisodeSpecification(IEpisodeService episodeService, ISeriesRepository seriesRepository, Logger logger)
        {
            _episodeService = episodeService;
            _seriesRepository = seriesRepository;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Series is not monitored";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject)
        {
            if (!subject.Series.Monitored)
            {
                _logger.Debug("{0} is present in the DB but not tracked. skipping.", subject.Series.Title);
                return false;
            }

            //return monitored if any of the episodes are monitored
            if (subject.Episodes.Any(episode => !episode.Ignored))
            {
                return true;
            }

            _logger.Debug("All episodes are ignored. skipping.");
            return false;
        }
    }
}
