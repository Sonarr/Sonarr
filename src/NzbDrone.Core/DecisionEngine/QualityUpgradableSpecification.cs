using NLog;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IQualityUpgradableSpecification
    {
        bool IsUpgradable(Profile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool CutoffNotMet(Profile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality);
    }

    public class QualityUpgradableSpecification : IQualityUpgradableSpecification
    {
        private readonly Logger _logger;

        public QualityUpgradableSpecification(Logger logger)
        {
            _logger = logger;
        }

        public bool IsUpgradable(Profile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            if (newQuality != null)
            {
                int compare = new QualityModelComparer(profile).Compare(newQuality, currentQuality);
                if (compare <= 0)
                {
                    _logger.Debug("existing item has better or equal quality. skipping");
                    return false;
                }

                if (IsRevisionUpgrade(currentQuality, newQuality))
                {
                    return true;
                }
            }

            return true;
        }

        public bool CutoffNotMet(Profile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            int compare = new QualityModelComparer(profile).Compare(currentQuality.Quality, profile.Cutoff);

            if (compare >= 0)
            {
                if (newQuality != null && IsRevisionUpgrade(currentQuality, newQuality))
                {
                    return true;
                }

                _logger.Debug("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }

        public bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            int compare = newQuality.Revision.CompareTo(currentQuality.Revision);

            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                _logger.Debug("New quality is a better revision for existing quality");
                return true;
            }

            return false;
        }
    }
}
