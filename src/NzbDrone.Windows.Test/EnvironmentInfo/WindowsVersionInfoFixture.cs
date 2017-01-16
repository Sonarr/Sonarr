using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Windows.EnvironmentInfo;

namespace NzbDrone.Windows.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Win")]
    public class WindowsVersionInfoFixture : TestBase<WindowsVersionInfo>
    {
        [Test]
        public void should_get_windows_version()
        {
            var info = Subject.Read();
            info.Version.Should().NotBeNullOrWhiteSpace();
            info.Name.Should().Contain("Windows");
            info.FullName.Should().Contain("Windows");
            info.FullName.Should().Contain("NT");
            info.FullName.Should().Contain(info.Version);
        }
    }
}
