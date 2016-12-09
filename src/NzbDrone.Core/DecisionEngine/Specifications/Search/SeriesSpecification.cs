using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SeriesSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SeriesSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            _logger.Debug("Checking if series matches searched series");

            if (remoteEpisode.Series.Id != searchCriteria.Series.Id)
            {
                _logger.Debug("Series {0} does not match {1}", remoteEpisode.Series, searchCriteria.Series);
                return Decision.Reject("Wrong series");
            }

            return Decision.Accept();
        }
    }
}