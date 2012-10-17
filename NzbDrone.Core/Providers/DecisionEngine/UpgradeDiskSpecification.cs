using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class UpgradeDiskSpecification
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly QualityUpgradeSpecification _qualityUpgradeSpecification;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public UpgradeDiskSpecification(EpisodeProvider episodeProvider, QualityUpgradeSpecification qualityUpgradeSpecification)
        {
            _episodeProvider = episodeProvider;
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
                if (!_qualityUpgradeSpecification.IsSatisfiedBy(new Quality { QualityType = file.Quality, Proper = file.Proper }, subject.Quality, subject.Series.QualityProfile.Cutoff))
                    return false;
            }

            return true;
        }
    }
}
