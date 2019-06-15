using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests.StringExtensionTests
{
    [TestFixture]
    public class FirstCharacterToLowerFixture
    {
        [TestCase("Hello", "hello")]
        [TestCase("CamelCase", "camelCase")]
        [TestCase("A Full Sentence", "a Full Sentence")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void should_lower_case_first_character(string input, string expected)
        {
            input.FirstCharToLower().Should().Be(expected);
        }

        [Test]
        public void should_lower_case_first_character_regardless_of_culture()
        {
            var current = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            try
            {
                "InfInite".FirstCharToLower().Should().Be("infInite");
            }
            finally
            {
                CultureInfo.CurrentCulture = current;
            }
        }
    }
}
