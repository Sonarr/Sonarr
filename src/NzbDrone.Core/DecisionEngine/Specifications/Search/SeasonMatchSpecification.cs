using NLog;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SeasonMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly ISceneMappingService _sceneMappingService;

        public SeasonMatchSpecification(ISceneMappingService sceneMappingService, Logger logger)
        {
            _logger = logger;
            _sceneMappingService = sceneMappingService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var singleEpisodeSpec = searchCriteria as SeasonSearchCriteria;
            if (singleEpisodeSpec == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (singleEpisodeSpec.SeasonNumber != remoteEpisode.ParsedEpisodeInfo.SeasonNumber)
            {
                _logger.Debug("Season number does not match searched season number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeason, "Wrong season");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
