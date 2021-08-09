using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser
{
    public class IsoLanguage
    {
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public string CountryCode { get; set; }
        public Language Language { get; set; }

        public IsoLanguage(string twoLetterCode, string countryCode, string threeLetterCode, Language language)
        {
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
            CountryCode = countryCode;
            Language = language;
        }
    }
}
