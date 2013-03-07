using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class LanguageSpecification : IFetchableSpecification
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

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            _logger.Trace("Checking if report meets language requirements. {0}", subject.Language);
            if (subject.Language != LanguageType.English)
            {
                _logger.Trace("Report Language: {0} rejected because it is not English", subject.Language);
                return false;
            }

            return true;
        }
    }
}
