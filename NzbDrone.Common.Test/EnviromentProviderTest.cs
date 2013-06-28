using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class IAppDirectoryInfoTest : TestBase<AppDirectoryInfo>
    {

        [Test]
        public void StartupPath_should_not_be_empty()
        {
            Subject.StartUpPath.Should().NotBeBlank();
            Path.IsPathRooted(Subject.StartUpPath).Should().BeTrue("Path is not rooted");

        }

        [Test]
        public void ApplicationPath_should_not_be_empty()
        {
            Subject.WorkingDirectory.Should().NotBeBlank();
            Path.IsPathRooted(Subject.WorkingDirectory).Should().BeTrue("Path is not rooted");
        }



        [Test]
        public void IsProduction_should_return_false_when_run_within_nunit()
        {
            RuntimeInfo.IsProduction.Should().BeFalse();
        }
    }
}
