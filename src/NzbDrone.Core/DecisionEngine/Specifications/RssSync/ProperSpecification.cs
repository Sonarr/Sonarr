using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class ProperSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public ProperSpecification(UpgradableSpecification upgradableSpecification, IConfigService configService, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                return DownloadSpecDecision.Accept();
            }

            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;

            if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotPrefer)
            {
                _logger.Debug("Propers are not preferred, skipping check");
                return DownloadSpecDecision.Accept();
            }

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (_upgradableSpecification.IsRevisionUpgrade(file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    if (downloadPropersAndRepacks == ProperDownloadTypes.DoNotUpgrade)
                    {
                        _logger.Debug("Auto downloading of propers is disabled");
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.PropersDisabled, "Proper downloading is disabled");
                    }

                    if (file.DateAdded < DateTime.Today.AddDays(-7))
                    {
                        _logger.Debug("Proper for old file, rejecting: {0}", subject);
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.ProperForOldFile, "Proper for old file");
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
