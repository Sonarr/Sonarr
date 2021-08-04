using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Test.EnvironmentInfo
{
    [TestFixture]
    public class BuildInfoFixture
    {
        [Test]
        public void should_return_version()
        {
            BuildInfo.Version.Major.Should().BeOneOf(4, 10);
        }

        [Test]
        public void should_get_branch()
        {
            BuildInfo.Branch.Should().NotBe("unknow");
            BuildInfo.Branch.Should().NotBeNullOrWhiteSpace();
        }
    }
}
