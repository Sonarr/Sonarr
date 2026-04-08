using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AirDateSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IReleaseProfileService _releaseProfileService;
        private readonly ITermMatcherService _termMatcherService;

        public AirDateSpecification(ITermMatcherService termMatcherService, IReleaseProfileService releaseProfileService, Logger logger)
        {
            _logger = logger;
            _releaseProfileService = releaseProfileService;
            _termMatcherService = termMatcherService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            _logger.Debug("Checking if release meets air date restrictions: {ReleaseTitle}", subject);

            var releaseProfiles = _releaseProfileService.EnabledForTags(subject.Series.Tags, subject.Release.IndexerId);

            if (releaseProfiles.Empty())
            {
                _logger.Debug("No Release Profile, accepting");
                return DownloadSpecDecision.Accept();
            }

            var bestProfile = releaseProfiles
                .OrderByDescending(p => p.AirDateRestriction ? 1 : 0)
                .ThenByDescending(p => p.AirDateGracePeriod)
                .First();

            if (!bestProfile.AirDateRestriction)
            {
                _logger.Debug("Release Profile does not prevent grabbing before release date, accepting");
                return DownloadSpecDecision.Accept();
            }

            var releaseDate = subject.Release.PublishDate;
            var gracePeriod = bestProfile.AirDateGracePeriod;

            foreach (var episode in subject.Episodes)
            {
                var airDate = episode.AirDateUtc;

                if (!airDate.HasValue)
                {
                    _logger.Debug("No air date available, rejecting");
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.BeforeAirDate, "No air date available");
                }

                var adjustedAirDate = airDate.Value.AddDays(gracePeriod);

                if (releaseDate < adjustedAirDate)
                {
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.BeforeAirDate, "Release date {0} is before adjusted air date of {1} (Air Date: {2}. Grace period {3} days)", releaseDate, adjustedAirDate, airDate, gracePeriod);
                }
            }

            _logger.Debug("All episodes within air date limitations, allowing");
            return DownloadSpecDecision.Accept();
        }

        private ReleaseProfile FindBestProfile(List<ReleaseProfile> releaseProfiles)
        {
            return releaseProfiles
                .OrderBy(p => p.AirDateRestriction ? 0 : 1)
                .ThenBy(p => p.AirDateGracePeriod)
                .ThenBy(p => p.AirDateRestriction ? 0 : 1)
                .FirstOrDefault();
        }
    }
}
