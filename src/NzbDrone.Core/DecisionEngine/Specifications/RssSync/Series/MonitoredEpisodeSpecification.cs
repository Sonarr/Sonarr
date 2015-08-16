using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;


namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync.Series
{
    public class MonitoredEpisodeSpecification : BaseSeriesSpecification
    {
        private readonly Logger _logger;

        public MonitoredEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public override RejectionType Type { get { return RejectionType.Permanent; } }

        public override Decision IsSatisfiedBy(RemoteEpisode subject, SeriesSearchCriteriaBase searchCriteria)
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
                _logger.Debug("{0} is present in the DB but not tracked. skipping.", subject.Series);
                return Decision.Reject("Series is not monitored");
            }

            var monitoredCount = subject.Episodes.Count(episode => episode.Monitored);
            if (monitoredCount == subject.Episodes.Count)
            {
                return Decision.Accept();
            }

            _logger.Debug("Only {0}/{1} episodes are monitored. skipping.", monitoredCount, subject.Episodes.Count);
            return Decision.Reject("Episode is not monitored");
        }
    }
}
