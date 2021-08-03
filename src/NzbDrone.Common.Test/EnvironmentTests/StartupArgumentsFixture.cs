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
            var args = new StartupContext(new string[0]);
            args.Flags.Should().BeEmpty();
        }

        [TestCase("/t")]
        [TestCase(" /t")]
        [TestCase(" /T")]
        [TestCase(" /t  ")]
        public void should_parse_single_flag(string arg)
        {
            var args = new StartupContext(new[] { arg });
            args.Flags.Should().HaveCount(1);
            args.Flags.Contains("t").Should().BeTrue();
        }

        [TestCase("/key=value")]
        [TestCase("/KEY=value")]
        [TestCase(" /key=\"value\"")]
        public void should_parse_args_with_alues(string arg)
        {
            var args = new StartupContext(new[] { arg });
            args.Args.Should().HaveCount(1);
            args.Args["key"].Should().Be("value");
        }

        [TestCase("/data=test", "/data=test")]
        [TestCase("/Data=/a/b/c", "/data=/a/b/c")]
        public void should_preserver_data(string arg, string preserved)
        {
            var args = new StartupContext(new[] { arg });
            args.PreservedArguments.Should().Be(preserved);
        }

        [TestCase("/nobrowser", "/nobrowser")]
        [TestCase("/Nobrowser", "/nobrowser")]
        [TestCase("-Nobrowser", "/nobrowser")]
        public void should_preserver_no_browser(string arg, string preserved)
        {
            var args = new StartupContext(new[] { arg });
            args.PreservedArguments.Should().Be(preserved);
        }

        [Test]
        public void should_preserver_both()
        {
            var args = new StartupContext(new[] { "/data=test", "/Nobrowser" });
            args.PreservedArguments.Should().Be("/data=test /nobrowser");
        }
    }
}
