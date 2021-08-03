using System.Linq;
using NLog;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SingleEpisodeSearchMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly ISceneMappingService _sceneMappingService;

        public SingleEpisodeSearchMatchSpecification(ISceneMappingService sceneMappingService, Logger logger)
        {
            _logger = logger;
            _sceneMappingService = sceneMappingService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            var singleEpisodeSpec = searchCriteria as SingleEpisodeSearchCriteria;
            if (singleEpisodeSpec != null)
            {
                return IsSatisfiedBy(remoteEpisode, singleEpisodeSpec);
            }

            var animeEpisodeSpec = searchCriteria as AnimeEpisodeSearchCriteria;
            if (animeEpisodeSpec != null)
            {
                return IsSatisfiedBy(remoteEpisode, animeEpisodeSpec);
            }

            return Decision.Accept();
        }

        private Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SingleEpisodeSearchCriteria singleEpisodeSpec)
        {
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

        private Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, AnimeEpisodeSearchCriteria animeEpisodeSpec)
        {
            if (remoteEpisode.ParsedEpisodeInfo.FullSeason && !animeEpisodeSpec.IsSeasonSearch)
            {
                _logger.Debug("Full season result during single episode search, skipping.");
                return Decision.Reject("Full season pack");
            }

            return Decision.Accept();
        }
    }
}
