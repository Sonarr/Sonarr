using System.Linq;
using NLog;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class PendingSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly Logger _logger;

        public PendingSpecification(IPendingReleaseService pendingReleaseService,
                                  Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Temporary;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            // Skip this check for RSS sync and interactive searches,

            if (subject.ReleaseSource == ReleaseSourceType.Rss)
            {
                return DownloadSpecDecision.Accept();
            }

            if (information.SearchCriteria is { UserInvokedSearch: true })
            {
                _logger.Debug("Ignoring delay for user invoked search");
                return DownloadSpecDecision.Accept();
            }

            var pending = _pendingReleaseService.GetPendingQueue();

            var matchingEpisode = pending.Where(q => q.RemoteEpisode?.Series != null &&
                                                     q.RemoteEpisode.Series.Id == subject.Series.Id &&
                                                     q.RemoteEpisode.Episodes.Select(e => e.Id).Intersect(subject.Episodes.Select(e => e.Id)).Any())
                                       .ToList();

            if (matchingEpisode.Any())
            {
                _logger.Debug("Release containing at least one matching episode is already pending. Delaying pushed release");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MinimumAgeDelayPushed, "Release containing at least one matching episode is already pending, delaying pushed release");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
