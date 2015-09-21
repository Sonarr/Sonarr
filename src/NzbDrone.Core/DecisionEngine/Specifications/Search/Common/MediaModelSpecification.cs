using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search.Common
{
    public class MediaModelSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MediaModelSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public Decision IsSatisfiedBy(RemoteItem remoteItem, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            _logger.Debug("Checking if models matches searched model");

            if (remoteItem.Media.Id != searchCriteria.Media.Id)
            {
                _logger.Debug("Models {0} does not match {1}", remoteItem.Media, searchCriteria.Media);
                return Decision.Reject("Wrong models");
            }

            return Decision.Accept();
        }
    }
}