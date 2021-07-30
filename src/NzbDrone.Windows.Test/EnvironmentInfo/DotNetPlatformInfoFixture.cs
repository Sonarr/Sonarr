using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Windows.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Net")]
    public class DotNetPlatformInfoFixture : TestBase<PlatformInfo>
    {
        [Test]
        public void should_get_framework_version()
        {
            Subject.Version.Major.Should().Be(4);
            Subject.Version.Minor.Should().BeOneOf(0, 5, 6, 7, 8);
        }
    }
}
