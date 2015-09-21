using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Common
{
    public class NotSampleSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public NotSampleSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(RemoteItem subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release.Title.ToLower().Contains("sample") && subject.Release.Size < 70.Megabytes())
            {
                _logger.Debug("Sample release, rejecting.");
                return Decision.Reject("Sample");
            }

            return Decision.Accept();
        }
    }
}
