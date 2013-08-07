using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
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

        public string RejectionReason
        {
            get
            {
                return "Not English";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteriaBase)
        {
            _logger.Trace("Checking if report meets language requirements. {0}", subject.ParsedEpisodeInfo.Language);
            if (subject.ParsedEpisodeInfo.Language != Language.English)
            {
                _logger.Trace("Report Language: {0} rejected because it is not English", subject.ParsedEpisodeInfo.Language);
                return false;
            }

            return true;
        }
    }
}
