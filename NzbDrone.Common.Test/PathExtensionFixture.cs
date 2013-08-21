using System;
using System.Diagnostics;
using System.IO;
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

            fakeEnvironment.SetupGet(c => c.AppDataFolder).Returns(@"C:\NzbDrone\".AsOsAgnostic());

            fakeEnvironment.SetupGet(c => c.TempFolder).Returns(@"C:\Temp\".AsOsAgnostic());

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
        public void Clean_Path_Windows(string dirty, string clean)
        {
            WindowsOnly();

            var result = dirty.CleanFilePath();
            result.Should().Be(clean);
        }

        [TestCase(@"/test/", @"/test")]
        [TestCase(@"//test/", @"/test")]
        [TestCase(@"//test//", @"/test")]
        [TestCase(@"//test// ", @"/test")]
        [TestCase(@"//test//other// ", @"/test/other")]
        [TestCase(@"//test//other//file.ext ", @"/test/other/file.ext")]
        [TestCase(@"//CAPITAL//lower// ", @"/CAPITAL/lower")]
        public void Clean_Path_Linux(string dirty, string clean)
        {
            LinuxOnly();

            var result = dirty.CleanFilePath();
            result.Should().Be(clean);
        }

        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\\", @"C:\")]
        [TestCase(@"C:\Test", @"C:\Test\\")]
        [TestCase(@"C:\\\\\Test", @"C:\Test\\")]
        [TestCase(@"C:\Test\\\\", @"C:\Test\\")]
        [TestCase(@"C:\Test", @"C:\Test\\")]
        [TestCase(@"\\Server\pool", @"\\Server\pool")]
        [TestCase(@"\\Server\pool\", @"\\Server\pool")]
        [TestCase(@"\\Server\pool", @"\\Server\pool\")]
        [TestCase(@"\\Server\pool\", @"\\Server\pool\")]
        [TestCase(@"\\smallcheese\DRIVE_G\TV-C\Simspsons", @"\\smallcheese\DRIVE_G\TV-C\Simspsons")]
        public void paths_should_be_equal(string first, string second)
        {
            first.AsOsAgnostic().PathEquals(second.AsOsAgnostic()).Should().BeTrue();
        }

        [TestCase(@"c:\", @"C:\")]
        public void should_be_equal_windows_only(string first, string second)
        {
            WindowsOnly();
            first.PathEquals(second.AsOsAgnostic()).Should().BeTrue();
        }

        [TestCase(@"C:\Test", @"C:\Test2\")]
        [TestCase(@"C:\Test\Test", @"C:\TestTest\")]
        public void paths_should_not_be_equal(string first, string second)
        {
            first.AsOsAgnostic().PathEquals(second.AsOsAgnostic()).Should().BeFalse();
        }

        [Test]
        public void normalize_path_exception_empty()
        {
            Assert.Throws<ArgumentException>(() => "".CleanFilePath());
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void normalize_path_exception_null()
        {
            string nullPath = null;
            Assert.Throws<ArgumentException>(() => nullPath.CleanFilePath());
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void get_actual_casing_for_none_existing_file_should_throw()
        {
            WindowsOnly();
            Assert.Throws<DirectoryNotFoundException>(() => "C:\\InValidFolder\\invalidfile.exe".GetActualCasing());
        }

        [Test]
        public void get_actual_casing_should_return_actual_casing_for_local_file_in_windows()
        {
            WindowsOnly();
            var path = Process.GetCurrentProcess().MainModule.FileName;
            path.ToUpper().GetActualCasing().Should().Be(path);
            path.ToLower().GetActualCasing().Should().Be(path);
        }

        [Test]
        public void get_actual_casing_should_return_origibal_value_in_linux()
        {
            LinuxOnly();
            var path = Process.GetCurrentProcess().MainModule.FileName;
            path.GetActualCasing().Should().Be(path);
            path.GetActualCasing().Should().Be(path);
        }

        [Test]
        public void get_actual_casing_should_return_actual_casing_for_local_dir_in_windows()
        {
            WindowsOnly();
            var path = Directory.GetCurrentDirectory();
            path.ToUpper().GetActualCasing().Should().Be(path);
            path.ToLower().GetActualCasing().Should().Be(path);
        }


        [Test]
        public void get_actual_casing_should_return_original_value_in_linux()
        {
            LinuxOnly();
            var path = Directory.GetCurrentDirectory();
            path.GetActualCasing().Should().Be(path);
            path.GetActualCasing().Should().Be(path);
        }

        [Test]
        [Explicit]
        public void get_actual_casing_should_return_original_casing_for_shares()
        {
            var path = @"\\server\Pool\Apps";
            path.GetActualCasing().Should().Be(path);
        }

        [Test]
        public void AppDataDirectory_path_test()
        {
            GetIAppDirectoryInfo().GetAppDataPath().Should().BeEquivalentTo(@"C:\NzbDrone\".AsOsAgnostic());
        }

        [Test]
        public void Config_path_test()
        {
            GetIAppDirectoryInfo().GetConfigPath().Should().BeEquivalentTo(@"C:\NzbDrone\Config.xml".AsOsAgnostic());
        }

        [Test]
        public void Sanbox()
        {
            GetIAppDirectoryInfo().GetUpdateSandboxFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\".AsOsAgnostic());
        }

        [Test]
        public void GetUpdatePackageFolder()
        {
            GetIAppDirectoryInfo().GetUpdatePackageFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\".AsOsAgnostic());
        }

        [Test]
        public void GetUpdateClientFolder()
        {
            GetIAppDirectoryInfo().GetUpdateClientFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\NzbDrone.Update\".AsOsAgnostic());
        }

        [Test]
        public void GetUpdateClientExePath()
        {
            GetIAppDirectoryInfo().GetUpdateClientExePath().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone.Update.exe".AsOsAgnostic());
        }

        [Test]
        public void GetUpdateLogFolder()
        {
            GetIAppDirectoryInfo().GetUpdateLogFolder().Should().BeEquivalentTo(@"C:\NzbDrone\UpdateLogs\".AsOsAgnostic());
        }
    }
}
