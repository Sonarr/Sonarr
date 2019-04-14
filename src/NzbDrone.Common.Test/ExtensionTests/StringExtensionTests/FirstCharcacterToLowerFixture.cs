using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests.StringExtensionTests
{
    [TestFixture]
    public class FirstCharcacterToLowerFixture
    {
        [TestCase("Hello", "hello")]
        [TestCase("CamelCase", "camelCase")]
        [TestCase("A Full Sentence", "a Full Sentence")]
        public void should_lower_case_first_character(string input, string expected)
        {
            input.FirstCharToLower().Should().Be(expected);
        }
    }
}
