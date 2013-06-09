using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MonitoredEpisodeSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MonitoredEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Series is not monitored or Episode is ignored";
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
