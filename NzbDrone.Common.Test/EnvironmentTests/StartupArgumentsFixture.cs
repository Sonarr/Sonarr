using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EnvironmentTests
{
    [TestFixture]
    public class StartupArgumentsFixture : TestBase
    {
        [Test]
        public void empty_array_should_return_empty_flags()
        {
            var args = new StartupArguments(new string[0]);
            args.Flags.Should().BeEmpty();
        }

        [TestCase("/t")]
        [TestCase(" /t")]
        [TestCase(" /T")]
        [TestCase(" /t  ")]
        public void should_parse_single_flag(string arg)
        {
            var args = new StartupArguments(new[] { arg });
            args.Flags.Should().HaveCount(1);
            args.Flags.Contains("t").Should().BeTrue();
        }

    }
}
