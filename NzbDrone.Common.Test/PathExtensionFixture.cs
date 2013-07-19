using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class PathExtensionFixture : TestBase
    {

        private IAppFolderInfo GetIAppDirectoryInfo()
        {
            var fakeEnvironment = new Mock<IAppFolderInfo>();

            fakeEnvironment.SetupGet(c => c.AppDataFolder).Returns(@"C:\NzbDrone\");

            fakeEnvironment.SetupGet(c => c.TempFolder).Returns(@"C:\Temp\");

            return fakeEnvironment.Object;
        }

        [TestCase(@"c:\test\", @"c:\test")]
        [TestCase(@"c:\\test\\", @"c:\test")]
        [TestCase(@"C:\\Test\\", @"C:\Test")]
        [TestCase(@"C:\\Test\\Test\", @"C:\Test\Test")]
        [TestCase(@"\\Testserver\Test\", @"\\Testserver\Test")]
        [TestCase(@"\\Testserver\\Test\", @"\\Testserver\Test")]
        [TestCase(@"\\Testserver\Test\file.ext", @"\\Testserver\Test\file.ext")]
        [TestCase(@"\\Testserver\Test\file.ext\\", @"\\Testserver\Test\file.ext")]
        [TestCase(@"\\Testserver\Test\file.ext   \\", @"\\Testserver\Test\file.ext")]
        public void Normalize_Path_Windows(string dirty, string clean)
        {
            WindowsOnly();

            var result = dirty.CleanPath();
            result.Should().Be(clean);
        }

        [TestCase(@"/test/", @"/test")]
        [TestCase(@"//test/", @"/test")]
        [TestCase(@"//test//", @"/test")]
        [TestCase(@"//test// ", @"/test")]
        [TestCase(@"//test//other// ", @"/test/other")]
        [TestCase(@"//test//other//file.ext ", @"/test/other/file.ext")]
        [TestCase(@"//CAPITAL//lower// ", @"/CAPITAL/lower")]
        public void Normalize_Path_Linux(string dirty, string clean)
        {
            LinuxOnly();

            var result = dirty.CleanPath();
            result.Should().Be(clean);
        }

        [Test]
        public void normalize_path_exception_empty()
        {
            Assert.Throws<ArgumentException>(() => "".CleanPath());
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void normalize_path_exception_null()
        {
            string nullPath = null;
            Assert.Throws<ArgumentException>(() => nullPath.CleanPath());
            ExceptionVerification.ExpectedWarns(1);
        }


        [Test]
        public void AppDataDirectory_path_test()
        {
            GetIAppDirectoryInfo().GetAppDataPath().Should().BeEquivalentTo(@"C:\NzbDrone\");
        }


        [Test]
        public void Config_path_test()
        {
            GetIAppDirectoryInfo().GetConfigPath().Should().BeEquivalentTo(@"C:\NzbDrone\Config.xml");
        }

        [Test]
        public void Sanbox()
        {
            GetIAppDirectoryInfo().GetUpdateSandboxFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\");
        }

        [Test]
        public void GetUpdatePackageFolder()
        {
            GetIAppDirectoryInfo().GetUpdatePackageFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\");
        }

        [Test]
        public void GetUpdateClientFolder()
        {
            GetIAppDirectoryInfo().GetUpdateClientFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\NzbDrone.Update\");
        }

        [Test]
        public void GetUpdateClientExePath()
        {
            GetIAppDirectoryInfo().GetUpdateClientExePath().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone.Update.exe");
        }

        [Test]
        public void GetUpdateLogFolder()
        {
            GetIAppDirectoryInfo().GetUpdateLogFolder().Should().BeEquivalentTo(@"C:\NzbDrone\UpdateLogs\");
        }
    }
}
