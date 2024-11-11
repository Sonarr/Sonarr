using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RetentionSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public RetentionSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release.DownloadProtocol != Indexers.DownloadProtocol.Usenet)
            {
                _logger.Debug("Not checking retention requirement for non-usenet report");
                return DownloadSpecDecision.Accept();
            }

            var age = subject.Release.Age;
            var retention = _configService.Retention;

            _logger.Debug("Checking if report meets retention requirements. {0}", age);
            if (retention > 0 && age > retention)
            {
                _logger.Debug("Report age: {0} rejected by user's retention limit", age);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MaximumAge, "Older than configured retention");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
