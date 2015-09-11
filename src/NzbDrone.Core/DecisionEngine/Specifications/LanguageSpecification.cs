using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class LanguageSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public LanguageSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var wantedLanguage = subject.Series.Profile.Value.Language;
            
            _logger.Debug("Checking if report meets language requirements. {0}", subject.ParsedEpisodeInfo.Language);

            if (subject.ParsedEpisodeInfo.Language != wantedLanguage)
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted, wanted {1}", subject.ParsedEpisodeInfo.Language, wantedLanguage);
                return Decision.Reject("{0} is wanted, but found {1}", wantedLanguage, subject.ParsedEpisodeInfo.Language);
            }

            var allowSubtitled = subject.Series.Profile.Value.AllowSubtitled;
            _logger.Debug("Checking if report meets subtitle requirements. {0}", subject.ParsedEpisodeInfo.IsSubtitled);

            if (subject.ParsedEpisodeInfo.IsSubtitled && !allowSubtitled)
            {
                _logger.Debug("Report Language: rejected because it is sub-titled");
                return Decision.Reject("Sub-titled not wanted");
            }

            return Decision.Accept();
        }
    }
}
