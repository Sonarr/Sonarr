using NLog;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IUpgradableSpecification
    {
        bool IsUpgradable(Profile profile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, QualityModel newQuality, Language newLanguage);
        bool CutoffNotMet(Profile profile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, QualityModel newQuality = null);
        bool IsRevisionUpgrade(QualityModel currentQuality, QualityModel newQuality);
    }

    public class UpgradableSpecification : IUpgradableSpecification
    {
        private readonly Logger _logger;

        public UpgradableSpecification(Logger logger)
        {
            _logger = logger;
        }

        private bool IsLanguageUpgradable(LanguageProfile profile, Language currentLanguage, Language newLanguage = null) 
        {
            if (newLanguage != null)
            {
                var compare = new LanguageComparer(profile).Compare(newLanguage, currentLanguage);
                if (compare <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsQualityUpgradable(Profile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            if (newQuality != null)
            {
                var compare = new QualityModelComparer(profile).Compare(newQuality, currentQuality);
                if (compare <= 0)
                {
                    _logger.Debug("existing item has better quality. skipping");
                    return false;
                }
            }
            return true;
        }


        public bool IsUpgradable(Profile profile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, QualityModel newQuality, Language newLanguage)
        {          
            // If qualities are the same then check language
            if (newQuality != null && currentQuality == newQuality)
            {
                return IsLanguageUpgradable(languageProfile, currentLanguage, newLanguage);
            }

            // If quality is worse then always return false
            if (!IsQualityUpgradable(profile, currentQuality, newQuality))
            {
                _logger.Debug("existing item has better quality. skipping");
                return false;
            }

            return true;
        }

        public bool CutoffNotMet(Profile profile, LanguageProfile languageProfile, QualityModel currentQuality, Language currentLanguage, QualityModel newQuality = null)
        {
            var languageCompare = new LanguageComparer(languageProfile).Compare(currentLanguage, languageProfile.Cutoff);
            var qualityCompare = new QualityModelComparer(profile).Compare(currentQuality.Quality, profile.Cutoff);

            // If we can upgrade the language (it is not the cutoff) then doesn't matter the quality we can always get same quality with prefered language
            if (languageCompare < 0)
            {
                return true;
            }

            if (qualityCompare >= 0)
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
            var compare = newQuality.Revision.CompareTo(currentQuality.Revision);

            if (currentQuality.Quality == newQuality.Quality && compare > 0)
            {
                _logger.Debug("New quality is a better revision for existing quality");
                return true;
            }

            return false;
        }
    }
}
