using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser
{
    public class IsoLanguage
    {
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public string EnglishName { get; set; }
        public Language Language { get; set; }

        public IsoLanguage(string twoLetterCode, string threeLetterCode, string englishName, Language language)
        {
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
            EnglishName = englishName;
            Language = language;
        }
    }
}
