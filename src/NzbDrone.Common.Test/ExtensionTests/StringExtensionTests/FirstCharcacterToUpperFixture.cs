using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests.StringExtensionTests
{
    [TestFixture]
    public class FirstCharcacterToUpperFixture
    {
        [TestCase("hello", "Hello")]
        [TestCase("camelCase", "CamelCase")]
        [TestCase("a full sentence", "A full sentence")]
<<<<<<< HEAD
        [TestCase("", "")]
        [TestCase(null, "")]
=======
>>>>>>> b6072c2d3... fixup! New: Limit Custom Scripts to specified scripts folder
        public void should_capitalize_first_character(string input, string expected)
        {
            input.FirstCharToUpper().Should().Be(expected);
        }
    }
}
