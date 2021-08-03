using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface IUpgradableSpecification
    {
        bool IsUpgradable(QualityProfile profile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, int currentScore, QualityModel newQuality, Language newLanguage, int newScore);
        bool QualityCutoffNotMet(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null);
        bool LanguageCutoffNotMet(LanguageProfile languageProfile, Language currentLanguage);
        bool CutoffNotMet(QualityProfile profile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, int currentScore, QualityModel newQuality = null, int newScore = 0);
        bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality);
        bool IsUpgradeAllowed(QualityProfile qualityProfile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, QualityModel newQuality, Language newLanguage);
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

        public bool IsUpgradable(QualityProfile qualityProfile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, int currentScore, QualityModel newQuality, Language newLanguage, int newScore)
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

            // Reject unless the user does not prefer propers/repacks and it's a revision downgrade.
            if (downloadPropersAndRepacks != ProperDownloadTypes.DoNotPrefer &&
                qualityRevisionComapre < 0)
            {
                _logger.Debug("Existing item has a better quality revision, skipping");
                return false;
            }

            var languageCompare = new LanguageComparer(languageProfile).Compare(newLanguage, currentLanguage);

            if (languageCompare > 0)
            {
                _logger.Debug("New item has a more preferred language");
                return true;
            }

            if (languageCompare < 0)
            {
                _logger.Debug("Existing item has better language, skipping");
                return false;
            }

            if (!IsPreferredWordUpgradable(currentScore, newScore))
            {
                _logger.Debug("Existing item has an equal or better preferred word score, skipping");
                return false;
            }

            _logger.Debug("New item has a better preferred word score");
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

        public bool LanguageCutoffNotMet(LanguageProfile languageProfile, Language currentLanguage)
        {
            var cutoff = languageProfile.UpgradeAllowed
                ? languageProfile.Cutoff
                : languageProfile.FirstAllowedLanguage();

            var languageCompare = new LanguageComparer(languageProfile).Compare(currentLanguage, cutoff);

            return languageCompare < 0;
        }

        public bool CutoffNotMet(QualityProfile profile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, int currentScore, QualityModel newQuality = null, int newScore = 0)
        {
            // If we can upgrade the language (it is not the cutoff) then the quality doesn't
            // matter as we can always get same quality with prefered language.
            if (LanguageCutoffNotMet(languageProfile, currentLanguage))
            {
                return true;
            }

            if (QualityCutoffNotMet(profile, currentQuality, newQuality))
            {
                return true;
            }

            if (IsPreferredWordUpgradable(currentScore, newScore))
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

        public bool IsUpgradeAllowed(QualityProfile qualityProfile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, QualityModel newQuality, Language newLanguage)
        {
            var isQualityUpgrade = new QualityModelComparer(qualityProfile).Compare(newQuality, currentQuality) > 0;
            var isLanguageUpgrade = new LanguageComparer(languageProfile).Compare(newLanguage, currentLanguage) > 0;

            if ((isQualityUpgrade && qualityProfile.UpgradeAllowed) ||
                (isLanguageUpgrade && languageProfile.UpgradeAllowed))
            {
                _logger.Debug("At least one profile allows upgrading");
                return true;
            }

            if (isQualityUpgrade && !qualityProfile.UpgradeAllowed)
            {
                _logger.Debug("Quality profile does not allow upgrades, skipping");
                return false;
            }

            if (isLanguageUpgrade && !languageProfile.UpgradeAllowed)
            {
                _logger.Debug("Language profile does not allow upgrades, skipping");
                return false;
            }

            return true;
        }
    }
}
