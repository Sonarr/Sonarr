using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests.StringExtensionTests
{
    [TestFixture]
    public class ReplaceEncodedUnicodeCharactersFixture
    {
        [TestCase("+\\u2b50", "\u2b50")]
        [TestCase("\\u2b50", "\u2b50")]
        [TestCase("+\\u00e9", "é")]
        [TestCase("\\u00e9", "é")]
        [TestCase("é", "é")]
        public void should_replace_encoded_unicode_character(string input, string expected)
        {
            input.ReplaceEncodedUnicodeCharacters().Should().Be(expected);
        }
    }
}
