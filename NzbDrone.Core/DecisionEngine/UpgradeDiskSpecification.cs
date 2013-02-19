using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class UpgradeDiskSpecification
    {
        private readonly QualityUpgradeSpecification _qualityUpgradeSpecification;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UpgradeDiskSpecification(QualityUpgradeSpecification qualityUpgradeSpecification)
        {
            _qualityUpgradeSpecification = qualityUpgradeSpecification;
        }

        public UpgradeDiskSpecification()
        {
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            foreach (var file in subject.Episodes.Select(c => c.EpisodeFile).Where(c => c != null))
            {
                logger.Trace("Comparing file quality with report. Existing file is {0} proper:{1}", file.Quality, file.Proper);
                if (!_qualityUpgradeSpecification.IsSatisfiedBy(new QualityModel { Quality = file.Quality, Proper = file.Proper }, subject.Quality, subject.Series.QualityProfile.Cutoff))
                    return false;

                if(subject.Quality.Proper && file.DateAdded < DateTime.Today.AddDays(-7))
                {
                    logger.Trace("Proper for old file, skipping: {0}", subject);
                    return false;
                }
            }

            return true;
        }
    }
}
