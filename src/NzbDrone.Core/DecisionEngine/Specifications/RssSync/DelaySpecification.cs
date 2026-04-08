using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class DelaySpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DelaySpecification(IPendingReleaseService pendingReleaseService,
                                  IDelayProfileService delayProfileService,
                                  IConfigService configService,
                                  Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _delayProfileService = delayProfileService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Temporary;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria is { UserInvokedSearch: true })
            {
                _logger.Debug("Ignoring delay for user invoked search");
                return DownloadSpecDecision.Accept();
            }

            var qualityProfile = subject.Series.QualityProfile.Value;
            var delayProfile = _delayProfileService.BestForTags(subject.Series.Tags);
            var delay = delayProfile.GetProtocolDelay(subject.Release.DownloadProtocol);
            var isPreferredProtocol = subject.Release.DownloadProtocol == delayProfile.PreferredProtocol;
            var preferPropersAndRepacks = _configService.DownloadPropersAndRepacks == ProperDownloadTypes.PreferAndUpgrade;

            if (delay == 0)
            {
                _logger.Debug("Delay Profile does not require a waiting period before download for {DownloadProtocol}.", subject.Release.DownloadProtocol);
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Delay Profile requires a waiting period of {DelayMinutes} minutes for {DownloadProtocol}", delay, subject.Release.DownloadProtocol);

            var qualityComparer = new QualityModelComparer(qualityProfile);

            if (isPreferredProtocol && preferPropersAndRepacks)
            {
                foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
                {
                    var currentQuality = file.Quality;
                    var newQuality = subject.ParsedEpisodeInfo.Quality;
                    var qualityCompare = qualityComparer.Compare(newQuality?.Quality, currentQuality.Quality);

                    if (qualityCompare == 0 && newQuality?.Revision.CompareTo(currentQuality.Revision) > 0)
                    {
                        _logger.Debug("New quality is a better revision for existing quality, skipping delay");
                        return DownloadSpecDecision.Accept();
                    }
                }
            }

            // If quality meets or exceeds the best allowed quality in the profile accept it immediately
            if (delayProfile.BypassIfHighestQuality)
            {
                var bestQualityInProfile = qualityProfile.LastAllowedQuality();
                var isBestInProfile = qualityComparer.Compare(subject.ParsedEpisodeInfo.Quality.Quality, bestQualityInProfile) >= 0;

                if (isBestInProfile && isPreferredProtocol)
                {
                    _logger.Debug("Quality is highest in profile for preferred protocol, will not delay");
                    return DownloadSpecDecision.Accept();
                }
            }

            // If quality meets or exceeds the best allowed quality in the profile accept it immediately
            if (delayProfile.BypassIfAboveCustomFormatScore)
            {
                var score = subject.CustomFormatScore;
                var minimum = delayProfile.MinimumCustomFormatScore;

                if (score >= minimum && isPreferredProtocol)
                {
                    _logger.Debug("Custom format score ({CustomFormatScore}) meets minimum ({MinimumScore}) for preferred protocol, will not delay", score, minimum);
                    return DownloadSpecDecision.Accept();
                }
            }

            var episodeIds = subject.Episodes.Select(e => e.Id);

            var oldest = _pendingReleaseService.OldestPendingRelease(subject.Series.Id, episodeIds.ToArray());

            if (oldest != null && oldest.Release.AgeMinutes > delay)
            {
                _logger.Debug("Oldest pending release {ReleaseTitle} has been delayed for {AgeMinutes}, longer than the set delay of {DelayMinutes}. Release will be accepted", oldest.Release.Title, oldest.Release.AgeMinutes, delay);
                return DownloadSpecDecision.Accept();
            }

            if (subject.Release.AgeMinutes < delay)
            {
                _logger.Debug("Waiting for better quality release, There is a {DelayMinutes} minute delay on {DownloadProtocol}", delay, subject.Release.DownloadProtocol);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MinimumAgeDelay, "Waiting for better quality release");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
