using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Common
{
    public class QualityAllowedByProfileSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public QualityAllowedByProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteItem subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if report meets quality requirements. {0}", subject.ParsedInfo.Quality);
            if (!subject.Media.Profile.Value.Items.Exists(v => v.Allowed && v.Quality == subject.ParsedInfo.Quality.Quality))
            {
                _logger.Debug("Quality {0} rejected by Series' quality profile", subject.ParsedInfo.Quality);
                return Decision.Reject("{0} is not wanted in profile", subject.ParsedInfo.Quality.Quality);
            }

            return Decision.Accept();
        }
    }
}
