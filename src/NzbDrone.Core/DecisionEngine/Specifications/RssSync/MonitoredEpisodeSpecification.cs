using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class MonitoredEpisodeSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MonitoredEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                if (!searchCriteria.MonitoredEpisodesOnly)
                {
                    _logger.Debug("Skipping monitored check during search");
                    return Decision.Accept();
                }
            }

            if (!subject.Series.Monitored)
            {
                _logger.Debug("{0} is present in the DB but not tracked. Rejecting", subject.Series);
                return Decision.Reject("Series is not monitored");
            }

            var monitoredCount = subject.Episodes.Count(episode => episode.Monitored);
            if (monitoredCount == subject.Episodes.Count)
            {
                return Decision.Accept();
            }

            if (subject.Episodes.Count == 1)
            {
                _logger.Debug("Episode is not monitored. Rejecting", monitoredCount, subject.Episodes.Count);
                return Decision.Reject("Episode is not monitored");
            }

            if (monitoredCount == 0)
            {
                _logger.Debug("No episodes in the release are monitored. Rejecting", monitoredCount, subject.Episodes.Count);
            }
            else
            {
                _logger.Debug("Only {0}/{1} episodes in the release are monitored. Rejecting", monitoredCount, subject.Episodes.Count);
            }

            return Decision.Reject("One or more episodes is not monitored");
        }
    }
}
