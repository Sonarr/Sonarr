using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class DailyEpisodeMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IEpisodeService _episodeService;

        public DailyEpisodeMatchSpecification(Logger logger, IEpisodeService episodeService)
        {
            _logger = logger;
            _episodeService = episodeService;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            var dailySearchSpec = searchCriteria as DailyEpisodeSearchCriteria;

            if (dailySearchSpec == null) return Decision.Accept();

            var episode = _episodeService.GetEpisode(dailySearchSpec.Series.Id, dailySearchSpec.AirDate.ToString(Episode.AIR_DATE_FORMAT));

            if (!remoteEpisode.ParsedEpisodeInfo.IsDaily || remoteEpisode.ParsedEpisodeInfo.AirDate != episode.AirDate)
            {
                _logger.Debug("Episode AirDate does not match searched episode number, skipping.");
                return Decision.Reject("Episode does not match");
            }

            return Decision.Accept();
        }
    }
}