using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Windows.EnvironmentInfo;

namespace NzbDrone.Windows.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Win")]
    public class DotNetPlatformInfoFixture : TestBase<DotNetPlatformInfo>
    {
        [Test]
        public void should_get_framework_version()
        {
            Subject.Version.Major.Should().Be(4);
            Subject.Version.Minor.Should().BeOneOf(0, 5, 6);
        }
    }
}
