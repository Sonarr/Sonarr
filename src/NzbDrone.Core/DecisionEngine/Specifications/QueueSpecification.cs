using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QueueSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IQueueService _queueService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                  UpgradableSpecification upgradableSpecification,
                                  ICustomFormatCalculationService formatService,
                                  IConfigService configService,
                                  Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
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

                var queuedItemCustomFormats = _formatService.ParseCustomFormat(remoteEpisode, (long)queueItem.Size);

                _logger.Debug("Checking if existing release in queue meets cutoff. Queued: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                    remoteEpisode.ParsedEpisodeInfo.Quality,
                    queuedItemCustomFormats,
                    subject.ParsedEpisodeInfo.Quality))
                {
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueCutoffMet, "Release in queue already meets cutoff: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(qualityProfile,
                    remoteEpisode.ParsedEpisodeInfo.Quality,
                    queuedItemCustomFormats,
                    subject.ParsedEpisodeInfo.Quality,
                    subject.CustomFormats);

                switch (upgradeableRejectReason)
                {
                    case UpgradeableRejectReason.BetterQuality:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueHigherPreference, "Release in queue is of equal or higher preference: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                    case UpgradeableRejectReason.BetterRevision:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueHigherRevision, "Release in queue is of equal or higher revision: {0}", remoteEpisode.ParsedEpisodeInfo.Quality.Revision);

                    case UpgradeableRejectReason.QualityCutoff:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueCutoffMet, "Release in queue meets quality cutoff: {0}", qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                    case UpgradeableRejectReason.CustomFormatCutoff:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueCustomFormatCutoffMet, "Release in queue meets Custom Format cutoff: {0}", qualityProfile.CutoffFormatScore);

                    case UpgradeableRejectReason.CustomFormatScore:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueCustomFormatScore, "Release in queue has an equal or higher Custom Format score: {0}", qualityProfile.CalculateCustomFormatScore(queuedItemCustomFormats));

                    case UpgradeableRejectReason.MinCustomFormatScore:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueCustomFormatScoreIncrement, "Release in queue has Custom Format score within Custom Format score increment: {0}", qualityProfile.MinUpgradeFormatScore);

                    case UpgradeableRejectReason.UpgradesNotAllowed:
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueueUpgradesNotAllowed, "Release in queue and Quality Profile '{0}' does not allow upgrades", qualityProfile.Name);
                }

                if (_upgradableSpecification.IsRevisionUpgrade(remoteEpisode.ParsedEpisodeInfo.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    if (_configService.DownloadPropersAndRepacks == ProperDownloadTypes.DoNotUpgrade)
                    {
                        _logger.Debug("Auto downloading of propers is disabled");
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.QueuePropersDisabled, "Proper downloading is disabled");
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
