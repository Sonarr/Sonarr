using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SingleEpisodeSearchMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SingleEpisodeSearchMatchSpecification(Logger logger)
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

            var singleEpisodeSpec = searchCriteria as SingleEpisodeSearchCriteria;
            if (singleEpisodeSpec == null) return Decision.Accept();

            if (singleEpisodeSpec.SeasonNumber != remoteEpisode.ParsedEpisodeInfo.SeasonNumber)
            {
                _logger.Debug("Season number does not match searched season number, skipping.");
                return Decision.Reject("Wrong season");
            }

            if (!remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers.Any())
            {
                _logger.Debug("Full season result during single episode search, skipping.");
                return Decision.Reject("Full season pack");
            }

            if (!remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers.Contains(singleEpisodeSpec.EpisodeNumber))
            {
                _logger.Debug("Episode number does not match searched episode number, skipping.");
                return Decision.Reject("Wrong episode");
            }

            return Decision.Accept();
        }
    }
}