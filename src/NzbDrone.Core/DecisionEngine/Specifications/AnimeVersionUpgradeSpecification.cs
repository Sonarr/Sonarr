using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AnimeVersionUpgradeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AnimeVersionUpgradeSpecification(UpgradableSpecification upgradableSpecification, IConfigService configService, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var releaseGroup = subject.ParsedEpisodeInfo.ReleaseGroup;

            if (subject.Series.SeriesType != SeriesTypes.Anime)
            {
                return DownloadSpecDecision.Accept();
            }

            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;

            if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                _logger.Debug("Version upgrades are not preferred, skipping check");
                return DownloadSpecDecision.Accept();
            }

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (file == null)
                {
                    continue;
                }

                if (_upgradableSpecification.IsRevisionUpgrade(file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    if (file.ReleaseGroup.IsNullOrWhiteSpace())
                    {
                        _logger.Debug("Unable to compare release group, existing file's release group is unknown");
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.UnknownReleaseGroup, "Existing release group is unknown");
                    }

                    if (releaseGroup.IsNullOrWhiteSpace())
                    {
                        _logger.Debug("Unable to compare release group, release's release group is unknown");
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.UnknownReleaseGroup, "Release group is unknown");
                    }

                    if (file.ReleaseGroup != releaseGroup)
                    {
                        _logger.Debug("Existing Release group is: {0} - release's release group is: {1}", file.ReleaseGroup, releaseGroup);
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.ReleaseGroupDoesNotMatch, "{0} does not match existing release group {1}", releaseGroup, file.ReleaseGroup);
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
