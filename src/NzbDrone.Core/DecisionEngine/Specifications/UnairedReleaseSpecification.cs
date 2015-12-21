using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UnairedReleaseSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public UnairedReleaseSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (_configService.GrabUnairedReleases)
            {
                return Decision.Accept();
            }

            if (subject.Episodes.Any(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.After(subject.Release.PublishDate.ToUniversalTime())))
            {
                _logger.Debug("At least one episode has air date after publish date ({0})", subject.Release.PublishDate);
                return Decision.Reject("At least one episode has air date after publish date ({0})", subject.Release.PublishDate);
            }

            return Decision.Accept();
        }
    }
}
