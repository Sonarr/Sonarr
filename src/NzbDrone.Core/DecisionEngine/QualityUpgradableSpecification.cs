using NLog;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IQualityUpgradableSpecification
    {
        bool IsUpgradable(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool CutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool IsProperUpgrade(QualityModel currentQuality, QualityModel newQuality);
    }

    public class QualityUpgradableSpecification : IQualityUpgradableSpecification
    {
        private readonly Logger _logger;

        public QualityUpgradableSpecification(Logger logger)
        {
            _logger = logger;
        }

        public bool IsUpgradable(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            if (newQuality != null)
            {
                int compare = new QualityModelComparer(profile).Compare(newQuality, currentQuality);
                if (compare <= 0)
                {
                    _logger.Debug("existing item has better or equal quality. skipping");
                    return false;
                }

                if (IsProperUpgrade(currentQuality, newQuality))
                {
                    return true;
                }
            }

            return true;
        }

        public bool CutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            int compare = new QualityModelComparer(profile).Compare(currentQuality.Quality, profile.Cutoff);

            if (compare >= 0)
            {
                if (newQuality != null && IsProperUpgrade(currentQuality, newQuality))
                {
                    return true;
                }

                _logger.Debug("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }

        public bool IsProperUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            int compare = newQuality.Proper.CompareTo(currentQuality.Proper);

            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                _logger.Debug("New quality is a proper for existing quality");
                return true;
            }

            return false;
        }
    }
}
