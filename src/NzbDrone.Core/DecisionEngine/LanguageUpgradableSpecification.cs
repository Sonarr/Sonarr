using NLog;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public interface ILanguageUpgradableSpecification
    {
        bool IsUpgradable(Profile profile, LanguageModel currentLanguage, LanguageModel newLanguage = null);
        bool CutoffNotMet(Profile profile, LanguageModel currentLanguage, LanguageModel newLanguage = null);
        bool IsRevisionUpgrade(LanguageModel currentLanguage, LanguageModel newLanguage);
    }

    public class LanguageUpgradableSpecification : ILanguageUpgradableSpecification
    {
        private readonly Logger _logger;

        public LanguageUpgradableSpecification(Logger logger)
        {
            _logger = logger;
        }

        public bool IsUpgradable(Profile profile, LanguageModel currentLanguage, LanguageModel newLanguage = null)
        {
            if (newLanguage != null)
            {
                int compare = new LanguageModelComparer(profile).Compare(newLanguage, currentLanguage);
                if (compare <= 0)
                {
                    _logger.Debug("existing item has better or equal language. skipping");
                    return false;
                }

                if (IsRevisionUpgrade(currentLanguage, newLanguage))
                {
                    return true;
                }
            }

            return true;
        }

        public bool CutoffNotMet(Profile profile, LanguageModel currentLanguage, LanguageModel newLanguage = null)
        {
            int compare = new LanguageModelComparer(profile).Compare(currentLanguage.Language, profile.Languages.Find(v => v.Allowed == true).Language);

            if (compare >= 0)
            {
                if (newLanguage != null && IsRevisionUpgrade(currentLanguage, newLanguage))
                {
                    return true;
                }

                _logger.Debug("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }

        public bool IsRevisionUpgrade(LanguageModel currentLanguage, LanguageModel newLanguage)
        {
            int compare = newLanguage.Revision.CompareTo(currentLanguage.Revision);

            if (currentLanguage.Language == newLanguage.Language && compare > 0)
            {
                _logger.Debug("New language is a better revision for existing quality");
                return true;
            }

            return false;
        }
    }
}
