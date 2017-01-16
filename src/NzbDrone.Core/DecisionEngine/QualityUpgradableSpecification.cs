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
            var compare = new QualityModelComparer(profile).Compare(currentQuality.Quality, profile.Cutoff);

            if (compare < 0)
            {
                return true;
            }

            if (newQuality != null && IsRevisionUpgrade(currentQuality, newQuality))
            {
                return true;
            }
            
            return false;

        }

        public bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            var compare = newQuality.Revision.CompareTo(currentQuality.Revision);

            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                return true;
            }

            return false;
        }
    }
}
