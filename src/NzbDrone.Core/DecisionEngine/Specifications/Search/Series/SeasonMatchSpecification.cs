using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search.Series
{
    public class SeasonMatchSpecification : BaseSeriesSpecification
    {
        private readonly Logger _logger;

        public SeasonMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public override RejectionType Type { get { return RejectionType.Permanent; } }

        public override Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SeriesSearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            var singleEpisodeSpec = searchCriteria as SeasonSearchCriteria;
            if (singleEpisodeSpec == null) return Decision.Accept();

            if (singleEpisodeSpec.SeasonNumber != remoteEpisode.ParsedEpisodeInfo.SeasonNumber)
            {
                _logger.Debug("Season number does not match searched season number, skipping.");
                return Decision.Reject("Wrong season");
            }

            return Decision.Accept();
        }
    }
}