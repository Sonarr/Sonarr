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
    }

    public class QualityUpgradableSpecification : IQualityUpgradableSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public QualityUpgradableSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
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

                if (currentQuality.Quality == newQuality.Quality && newQuality.Proper && _configService.AutoDownloadPropers)
                {
                    _logger.Trace("Upgrading existing item to proper.");
                    return true;
                }
            }

            return true;
        }

        public bool CutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            if (currentQuality.Quality >= profile.Cutoff)
            {
                _logger.Trace("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }
    }
}
