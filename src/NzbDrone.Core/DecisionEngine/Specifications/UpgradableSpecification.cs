using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface IUpgradableSpecification
    {
        bool IsUpgradable(QualityProfile profile, QualityModel currentQuality, List<CustomFormat> currentCustomFormats, QualityModel newQuality, List<CustomFormat> newCustomFormats);
        bool QualityCutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool CutoffNotMet(QualityProfile profile, QualityModel currentQuality, List<CustomFormat> currentCustomFormats, QualityModel newQuality = null);
        bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality);
        bool IsUpgradeAllowed(QualityProfile qualityProfile, QualityModel currentQuality, QualityModel newQuality);
    }

    public class UpgradableSpecification : IUpgradableSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public UpgradableSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        private bool IsPreferredWordUpgradable(int currentScore, int newScore)
        {
            _logger.Debug("Comparing preferred word score. Current: {0} New: {1}", currentScore, newScore);

            return newScore > currentScore;
        }

        public bool IsUpgradable(QualityProfile qualityProfile, QualityModel currentQuality, List<CustomFormat> currentCustomFormats, QualityModel newQuality, List<CustomFormat> newCustomFormats)
        {
            var qualityComparer = new QualityModelComparer(qualityProfile);
            var qualityCompare = qualityComparer.Compare(newQuality?.Quality, currentQuality.Quality);
            var downloadPropersAndRepacks = _configService.DownloadPropersAndRepacks;

            if (qualityCompare > 0)
            {
                _logger.Debug("New item has a better quality");
                return true;
            }

            if (qualityCompare < 0)
            {
                _logger.Debug("Existing item has better quality, skipping");
                return false;
            }

            var qualityRevisionComapre = newQuality?.Revision.CompareTo(currentQuality.Revision);

            // Accept unless the user doesn't want to prefer propers, optionally they can
            // use preferred words to prefer propers/repacks over non-propers/repacks.
            if (downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                qualityRevisionComapre > 0)
            {
                _logger.Debug("New item has a better quality revision");
                return true;
            }

            var currentFormatScore = qualityProfile.CalculateCustomFormatScore(currentCustomFormats);
            var newFormatScore = qualityProfile.CalculateCustomFormatScore(newCustomFormats);

            // Reject unless the user does not prefer propers/repacks and it's a revision downgrade.
            if (downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                qualityRevisionComapre < 0)
            {
                _logger.Debug("Existing item has a better quality revision, skipping");
                return false;
            }

            if (newFormatScore <= currentFormatScore)
            {
                _logger.Debug("New item's custom formats [{0}] do not improve on [{1}], skipping",
                              newCustomFormats.ConcatToString(),
                              currentCustomFormats.ConcatToString());
                return false;
            }

            _logger.Debug("New item has a better custom format score");
            return true;
        }

        public bool QualityCutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            var cutoff = profile.UpgradeAllowed ? profile.Cutoff : profile.FirststAllowedQuality().Id;
            var cutoffCompare = new QualityModelComparer(profile).Compare(currentQuality.Quality.Id, cutoff);

            if (cutoffCompare < 0)
            {
                return true;
            }

            if (newQuality != null && IsRevisionUpgrade(currentQuality, newQuality))
            {
                return true;
            }

            return false;
        }

        private bool CustomFormatCutoffNotMet(QualityProfile profile, List<CustomFormat> currentFormats)
        {
            var score = profile.CalculateCustomFormatScore(currentFormats);
            return score < profile.CutoffFormatScore;
        }

        public bool CutoffNotMet(QualityProfile profile, QualityModel currentQuality, List<CustomFormat> currentFormats, QualityModel newQuality = null)
        {
            if (QualityCutoffNotMet(profile, currentQuality, newQuality))
            {
                return true;
            }

            if (CustomFormatCutoffNotMet(profile, currentFormats))
            {
                return true;
            }

            _logger.Debug("Existing item meets cut-off. skipping.");

            return false;
        }

        public bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality)
        {
            var compare = newQuality.Revision.CompareTo(currentQuality.Revision);

            // Comparing the quality directly because we don't want to upgrade to a proper for a webrip from a webdl or vice versa
            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                _logger.Debug("New quality is a better revision for existing quality");
                return true;
            }

            return false;
        }

        public bool IsUpgradeAllowed(QualityProfile qualityProfile, QualityModel currentQuality, QualityModel newQuality)
        {
            var isQualityUpgrade = new QualityModelComparer(qualityProfile).Compare(newQuality, currentQuality) > 0;

            if (isQualityUpgrade && qualityProfile.UpgradeAllowed)
            {
                _logger.Debug("At least one profile allows upgrading");
                return true;
            }

            if (isQualityUpgrade && !qualityProfile.UpgradeAllowed)
            {
                _logger.Debug("Quality profile does not allow upgrades, skipping");
                return false;
            }

            return true;
        }
    }
}
