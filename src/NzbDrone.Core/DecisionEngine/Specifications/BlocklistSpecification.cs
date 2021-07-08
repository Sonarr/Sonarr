using NLog;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class BlocklistSpecification : IDecisionEngineSpecification
    {
        private readonly IBlocklistService _blocklistService;
        private readonly Logger _logger;

        public BlocklistSpecification(IBlocklistService blocklistService, Logger logger)
        {
            _blocklistService = blocklistService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (_blocklistService.Blocklisted(subject.Series.Id, subject.Release))
            {
                _logger.Debug("{0} is blocklisted, rejecting.", subject.Release.Title);
                return Decision.Reject("Release is blocklisted");
            }

            return Decision.Accept();
        }
    }
}
