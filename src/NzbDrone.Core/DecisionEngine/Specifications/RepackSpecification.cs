using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RepackSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public RepackSpecification(UpgradableSpecification upgradableSpecification, IConfigService configService, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (!subject.ParsedEpisodeInfo.Quality.Revision.IsRepack)
            {
                return Decision.Accept();
            }

            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;

            if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                _logger.Debug("Repacks are not preferred, skipping check");
                return Decision.Accept();
            }

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (_upgradableSpecification.IsRevisionUpgrade(file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotUpgrade)
                    {
                        _logger.Debug("Auto downloading of repacks is disabled");
                        return Decision.Reject("Repack downloading is disabled");
                    }

                    var releaseGroup = subject.ParsedEpisodeInfo.ReleaseGroup;
                    var fileReleaseGroup = file.ReleaseGroup;

                    if (fileReleaseGroup.IsNullOrWhiteSpace())
                    {
                        return Decision.Reject("Unable to determine release group for the existing file");
                    }

                    if (releaseGroup.IsNullOrWhiteSpace())
                    {
                        return Decision.Reject("Unable to determine release group for this release");
                    }

                    if (!fileReleaseGroup.Equals(releaseGroup, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.Debug(
                            "Release is a repack for a different release group. Release Group: {0}. File release group: {1}",
                            releaseGroup,
                            fileReleaseGroup);
                        return Decision.Reject(
                            "Release is a repack for a different release group. Release Group: {0}. File release group: {1}",
                            releaseGroup,
                            fileReleaseGroup);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
