using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SeasonMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SeasonMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Episode doesn't match";
            }
        }

        public bool IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteriaBase)
        {
            if (searchCriteriaBase == null)
            {
                return true;
            }

            var singleEpisodeSpec = searchCriteriaBase as SeasonSearchCriteria;
            if (singleEpisodeSpec == null) return true;

            if (singleEpisodeSpec.SeasonNumber != remoteEpisode.ParsedEpisodeInfo.SeasonNumber)
            {
                _logger.Trace("Season number does not match searched season number, skipping.");
                return false;
            }

            return true;
        }
    }
}