using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IQualityUpgradableSpecification
    {
        bool IsUpgradable(QualityModel currentQuality, QualityModel newQuality = null);
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

        public bool IsUpgradable(QualityModel currentQuality, QualityModel newQuality = null)
        {
            if (newQuality != null)
            {
                if (currentQuality >= newQuality)
                {
                    _logger.Trace("existing item has better or equal quality. skipping");
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
            if (currentQuality.Quality >= profile.Cutoff)
            {
                if (newQuality != null && IsProperUpgrade(currentQuality, newQuality))
                {
                    return true;
                }

                _logger.Trace("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }

        public bool IsProperUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            if (currentQuality.Quality == newQuality.Quality && newQuality > currentQuality)
            {
                _logger.Trace("New quality is a proper for existing quality");
                return true;
            }

            return false;
        }
    }
}
