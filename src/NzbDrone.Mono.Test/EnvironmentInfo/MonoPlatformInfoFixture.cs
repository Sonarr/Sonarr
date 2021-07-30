using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Mono")]
    public class MonoPlatformInfoFixture : TestBase<PlatformInfo>
    {
        [Test]
        public void should_get_framework_version()
        {
            Subject.Version.Major.Should().BeOneOf(4, 5, 6);
            if (Subject.Version.Major == 4)
            {
                Subject.Version.Minor.Should().BeOneOf(0, 5, 6);
            }
        }
    }
}
