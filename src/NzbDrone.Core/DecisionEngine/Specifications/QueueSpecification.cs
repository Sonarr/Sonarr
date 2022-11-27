using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QueueSpecification : IDecisionEngineSpecification
    {
        private readonly IQueueService _queueService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                  UpgradableSpecification upgradableSpecification,
                                  ICustomFormatCalculationService formatService,
                                  Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue();
            var matchingEpisode = queue.Where(q => q.RemoteEpisode?.Series != null &&
                                                   q.RemoteEpisode.Series.Id == subject.Series.Id &&
                                                   q.RemoteEpisode.Episodes.Select(e => e.Id).Intersect(subject.Episodes.Select(e => e.Id)).Any())
                                       .ToList();

            foreach (var queueItem in matchingEpisode)
            {
                var remoteEpisode = queueItem.RemoteEpisode;
                var qualityProfile = subject.Series.QualityProfile.Value;

                // To avoid a race make sure it's not FailedPending (failed awaiting removal/search).
                // Failed items (already searching for a replacement) won't be part of the queue since
                // it's a copy, of the tracked download, not a reference.

                if (queueItem.TrackedDownloadState == TrackedDownloadState.FailedPending)
                {
                    continue;
                }

                var queuedItemCustomFormats = _formatService.ParseCustomFormat(remoteEpisode.ParsedEpisodeInfo, subject.Series);

                _logger.Debug("Checking if existing release in queue meets cutoff. Queued: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                    remoteEpisode.ParsedEpisodeInfo.Quality,
                    queuedItemCustomFormats,
                    subject.ParsedEpisodeInfo.Quality))
                {
                    return Decision.Reject("Release in queue already meets cutoff: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                if (!_upgradableSpecification.IsUpgradable(qualityProfile,
                                                           remoteEpisode.ParsedEpisodeInfo.Quality,
                                                           remoteEpisode.CustomFormats,
                                                           subject.ParsedEpisodeInfo.Quality,
                                                           subject.CustomFormats))
                {
                    return Decision.Reject("Release in queue is of equal or higher preference: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);
                }

                _logger.Debug("Checking if profiles allow upgrading. Queued: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                if (!_upgradableSpecification.IsUpgradeAllowed(subject.Series.QualityProfile,
                                                               remoteEpisode.ParsedEpisodeInfo.Quality,
                                                               subject.ParsedEpisodeInfo.Quality))
                {
                    return Decision.Reject("Another release is queued and the Quality profile does not allow upgrades");
                }
            }

            return Decision.Accept();
        }
    }
}
