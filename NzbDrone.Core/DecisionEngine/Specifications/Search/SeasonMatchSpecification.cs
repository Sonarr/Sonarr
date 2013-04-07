using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SeasonMatchSpecification : IDecisionEngineSearchSpecification
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

        public bool IsSatisfiedBy(IndexerParseResult indexerParseResult, SearchDefinitionBase searchDefinitionBase)
        {
            var singleEpisodeSpec = searchDefinitionBase as SeasonSearchDefinition;
            if (singleEpisodeSpec == null) return true;

            if (singleEpisodeSpec.SeasonNumber != indexerParseResult.SeasonNumber)
            {
                _logger.Trace("Season number does not match searched season number, skipping.");
                return false;
            }
           
            return true;
        }
    }
}