using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class IsoLanguagesFixture : CoreTest
    {
        [TestCase("en")]
        [TestCase("eng")]
        [TestCase("en-US")]
        [TestCase("en-GB")]
        public void should_return_iso_language_for_English(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.English);
        }

        [TestCase("enus")]
        [TestCase("enusa")]
        [TestCase("wo")]
        [TestCase("fr-CA")]
        public void unknown_or_invalid_code_should_return_null(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().Be(null);
        }

        [TestCase("pt")]
        [TestCase("por")]
        [TestCase("pt-PT")]
        public void should_return_portuguese(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Portuguese);
        }

        [TestCase("de-AU")]
        public void should_not_return_portuguese(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().Be(null);
        }

        [TestCase("cze")]
        public void should_lookup_cze_via_alternate_iso_code_mapping(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Czech);
        }

        [TestCase("ur")]
        [TestCase("urd")]
        [TestCase("ur-PK")]
        public void should_return_urdu(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Urdu);
        }

        [TestCase("rm")]
        [TestCase("roh")]
        [TestCase("rm-CH")]
        public void should_return_romansh(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Romansh);
        }

        [TestCase("ka")]
        [TestCase("geo")]
        [TestCase("kat")]
        [TestCase("ka-GE")]
        public void should_return_georgian(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Georgian);
        }

        [TestCase("ga")]
        [TestCase("gle")]
        [TestCase("ga-IE")]
        public void should_return_irish(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Irish);
        }

        [TestCase("IT", "Italian")]
        [TestCase("ITA", "Italian")]
        [TestCase("ID", "Indonesian")]
        public void should_find_iso_language_regardless_of_culture(string isoCode, string languageName)
        {
            var current = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");

            try
            {
                var result = IsoLanguages.Find(isoCode);
                result.Should().NotBeNull();
                result.Language.Should().Be(Language.All.Single(l => l.Name == languageName));
            }
            finally
            {
                CultureInfo.CurrentCulture = current;
            }
        }
    }
}
