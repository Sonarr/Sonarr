using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class ProperSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public ProperSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, IConfigService configService, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _configService = configService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Proper for old episode";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                return true;
            }

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (_qualityUpgradableSpecification.IsProperUpgrade(file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    if (file.DateAdded < DateTime.Today.AddDays(-7))
                    {
                        _logger.Trace("Proper for old file, rejecting: {0}", subject);
                        return false;
                    }

                    if (!_configService.AutoDownloadPropers)
                    {
                        _logger.Trace("Auto downloading of propers is disabled");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
