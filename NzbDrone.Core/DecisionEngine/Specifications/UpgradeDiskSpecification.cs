using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Tv;

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

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            foreach (var file in subject.Episodes.Select(c => c.EpisodeFile).Where(c => c != null))
            {
                _logger.Trace("Comparing file quality with report. Existing file is {0}", file.Quality);
                if (!_qualityUpgradableSpecification.IsUpgradable(subject.Series.QualityProfile, file.Quality, subject.Quality))
                    return false;

                if (subject.Quality.Proper && file.DateAdded < DateTime.Today.AddDays(-7))
                {
                    _logger.Trace("Proper for old file, skipping: {0}", subject);
                    return false;
                }
            }

            return true;
        }
    }
}
