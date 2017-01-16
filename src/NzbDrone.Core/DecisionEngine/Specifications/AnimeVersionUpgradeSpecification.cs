using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AnimeVersionUpgradeSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public AnimeVersionUpgradeSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var releaseGroup = subject.ParsedEpisodeInfo.ReleaseGroup;

            if (subject.Series.SeriesType != SeriesTypes.Anime)
            {
                return Decision.Accept();
            }

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (_qualityUpgradableSpecification.IsRevisionUpgrade(file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    if (file.ReleaseGroup.IsNullOrWhiteSpace())
                    {
                        _logger.Debug("Unable to compare release group, existing file's release group is unknown");
                        return Decision.Reject("Existing release group is unknown");
                    }

                    if (releaseGroup.IsNullOrWhiteSpace())
                    {
                        _logger.Debug("Unable to compare release group, release's release group is unknown");
                        return Decision.Reject("Release group is unknown");
                    }

                    if (file.ReleaseGroup != releaseGroup)
                    {
                        _logger.Debug("Existing Release group is: {0} - release's release group is: {1}", file.ReleaseGroup, releaseGroup);
                        return Decision.Reject("{0} does not match existing release group {1}", releaseGroup, file.ReleaseGroup);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
