using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QualityAllowedByProfileSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public QualityAllowedByProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if report meets quality requirements. {0}", subject.ParsedEpisodeInfo.Quality);
            if (!subject.Series.Profile.Value.Items.Exists(v => v.Allowed && v.Quality == subject.ParsedEpisodeInfo.Quality.Quality))
            {
                _logger.Debug("Quality {0} rejected by Series' quality profile", subject.ParsedEpisodeInfo.Quality);
                return Decision.Reject("{0} is not wanted in profile", subject.ParsedEpisodeInfo.Quality.Quality);
            }

            return Decision.Accept();
        }
    }
}
