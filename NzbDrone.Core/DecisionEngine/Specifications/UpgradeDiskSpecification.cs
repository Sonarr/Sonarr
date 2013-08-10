using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Higher quality exists on disk";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                _logger.Trace("Comparing file quality with report. Existing file is {0}", file.Quality);

                if (!_qualityUpgradableSpecification.IsUpgradable(subject.Series.QualityProfile, file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    return false;
                }

                if (subject.ParsedEpisodeInfo.Quality.Proper && file.DateAdded < DateTime.Today.AddDays(-7))
                {
                    _logger.Trace("Proper for old file, skipping: {0}", subject);
                    return false;
                }
            }

            return true;
        }
    }
}
