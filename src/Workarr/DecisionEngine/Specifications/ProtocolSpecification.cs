using NLog;
using Workarr.Indexers;
using Workarr.IndexerSearch.Definitions;
using Workarr.Parser.Model;
using Workarr.Profiles.Delay;

namespace Workarr.DecisionEngine.Specifications
{
    public class ProtocolSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IDelayProfileService _delayProfileService;
        private readonly Logger _logger;

        public ProtocolSpecification(IDelayProfileService delayProfileService,
                                     Logger logger)
        {
            _delayProfileService = delayProfileService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var delayProfile = _delayProfileService.BestForTags(subject.Series.Tags);

            if (subject.Release.DownloadProtocol == DownloadProtocol.Usenet && !delayProfile.EnableUsenet)
            {
                _logger.Debug("[{0}] Usenet is not enabled for this series", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.ProtocolDisabled, "Usenet is not enabled for this series");
            }

            if (subject.Release.DownloadProtocol == DownloadProtocol.Torrent && !delayProfile.EnableTorrent)
            {
                _logger.Debug("[{0}] Torrent is not enabled for this series", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.ProtocolDisabled, "Torrent is not enabled for this series");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
