using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SingleEpisodeSearchMatchSpecification : IDecisionEngineSearchSpecification
    {
        private readonly Logger _logger;

        public SingleEpisodeSearchMatchSpecification(Logger logger)
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
            var singleEpisodeSpec = searchCriteriaBase as SingleEpisodeSearchCriteria;
            if (singleEpisodeSpec == null) return true;

            if (singleEpisodeSpec.SeasonNumber != remoteEpisode.ParsedEpisodeInfo.SeasonNumber)
            {
                _logger.Trace("Season number does not match searched season number, skipping.");
                return false;
            }

            if (!remoteEpisode.Episodes.Select(c => c.EpisodeNumber).Contains(singleEpisodeSpec.EpisodeNumber))
            {
                _logger.Trace("Episode number does not match searched episode number, skipping.");
                return false;
            }

            return true;
        }
    }
}