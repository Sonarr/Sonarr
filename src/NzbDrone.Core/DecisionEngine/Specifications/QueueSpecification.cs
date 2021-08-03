using System.Linq;
using NLog;
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
        private readonly IPreferredWordService _preferredWordServiceCalculator;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                  UpgradableSpecification upgradableSpecification,
                                  IPreferredWordService preferredWordServiceCalculator,
                                  Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = upgradableSpecification;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
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
                var languageProfile = subject.Series.LanguageProfile.Value;

                // To avoid a race make sure it's not FailedPending (failed awaiting removal/search).
                // Failed items (already searching for a replacement) won't be part of the queue since
                // it's a copy, of the tracked download, not a reference.

                if (queueItem.TrackedDownloadState == TrackedDownloadState.FailedPending)
                {
                    continue;
                }

                _logger.Debug("Checking if existing release in queue meets cutoff. Queued: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);
                var queuedItemPreferredWordScore = _preferredWordServiceCalculator.Calculate(subject.Series, queueItem.Title, subject.Release?.IndexerId ?? 0);

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                    languageProfile,
                    remoteEpisode.ParsedEpisodeInfo.Quality,
                    remoteEpisode.ParsedEpisodeInfo.Language,
                    queuedItemPreferredWordScore,
                    subject.ParsedEpisodeInfo.Quality,
                    subject.PreferredWordScore))
                {
                    return Decision.Reject("Release in queue already meets cutoff: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);

                if (!_upgradableSpecification.IsUpgradable(qualityProfile,
                                                           languageProfile,
                                                           remoteEpisode.ParsedEpisodeInfo.Quality,
                                                           remoteEpisode.ParsedEpisodeInfo.Language,
                                                           queuedItemPreferredWordScore,
                                                           subject.ParsedEpisodeInfo.Quality,
                                                           subject.ParsedEpisodeInfo.Language,
                                                           subject.PreferredWordScore))
                {
                    return Decision.Reject("Release in queue is of equal or higher preference: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);
                }

                _logger.Debug("Checking if profiles allow upgrading. Queued: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);

                if (!_upgradableSpecification.IsUpgradeAllowed(subject.Series.QualityProfile,
                                                               subject.Series.LanguageProfile,
                                                               remoteEpisode.ParsedEpisodeInfo.Quality,
                                                               remoteEpisode.ParsedEpisodeInfo.Language,
                                                               subject.ParsedEpisodeInfo.Quality,
                                                               subject.ParsedEpisodeInfo.Language))
                {
                    return Decision.Reject("Another release is queued and the Quality or Language profile does not allow upgrades");
                }
            }

            return Decision.Accept();
        }
    }
}
