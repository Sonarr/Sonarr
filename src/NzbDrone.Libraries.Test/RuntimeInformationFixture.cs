using System.Runtime.InteropServices;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Libraries.Test
{
    [TestFixture]
    public class RuntimeInformationFixture : TestBase
    {
        [Test]
        public void should_report_correct_osplatform()
        {
            var isWindows = OsInfo.IsWindows;
            var isLinux = OsInfo.IsLinux;
            var isOsx = OsInfo.IsOsx;

            RuntimeInformation.IsOSPlatform(OSPlatform.Windows).Should().Be(isWindows);
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux).Should().Be(isLinux && !isOsx);
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX).Should().Be(isOsx);
        }
    }
}
