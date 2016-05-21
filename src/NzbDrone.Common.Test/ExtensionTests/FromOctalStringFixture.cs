using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests
{
    [TestFixture]
    public class FromOctalStringFixture
    {
        [TestCase("\\040", " ")]
        [TestCase("\\043", "#")]
        [TestCase("\\101", "A")]
        public void should_convert_octal_character_string_to_ascii_string(string octalString, string expected)
        {
            octalString.FromOctalString().Should().Be(expected);
        }
    }
}
