using System.Diagnostics;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class IAppDirectoryInfoTest : TestBase<AppFolderInfo>
    {

        [Test]
        public void StartupPath_should_not_be_empty()
        {
            Subject.StartUpFolder.Should().NotBeBlank();
            Path.IsPathRooted(Subject.StartUpFolder).Should().BeTrue("Path is not rooted");

        }

        [Test]
        public void ApplicationPath_should_not_be_empty()
        {
            Subject.AppDataFolder.Should().NotBeBlank();
            Path.IsPathRooted(Subject.AppDataFolder).Should().BeTrue("Path is not rooted");
        }



        [Test]
        public void IsProduction_should_return_false_when_run_within_nunit()
        {
            RuntimeInfo.IsProduction.Should().BeFalse("Process name is " + Process.GetCurrentProcess().ProcessName);
        }
    }
}
