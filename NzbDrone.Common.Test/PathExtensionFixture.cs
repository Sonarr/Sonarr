using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class PathExtensionFixture : TestBase
    {

        private EnvironmentProvider GetEnvironmentProvider()
        {
            var fakeEnvironment = new Mock<EnvironmentProvider>();

            fakeEnvironment.SetupGet(c => c.WorkingDirectory).Returns(@"C:\NzbDrone\");

            fakeEnvironment.SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");

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
            GetEnvironmentProvider().GetAppDataPath().Should().BeEquivalentTo(@"C:\NzbDrone\App_Data\");
        }


        [Test]
        public void Config_path_test()
        {
            GetEnvironmentProvider().GetConfigPath().Should().BeEquivalentTo(@"C:\NzbDrone\Config.xml");
        }

        [Test]
        public void Sanbox()
        {
            GetEnvironmentProvider().GetUpdateSandboxFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\");
        }

        [Test]
        public void GetUpdatePackageFolder()
        {
            GetEnvironmentProvider().GetUpdatePackageFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\");
        }

        [Test]
        public void GetUpdateClientFolder()
        {
            GetEnvironmentProvider().GetUpdateClientFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\NzbDrone.Update\");
        }

        [Test]
        public void GetUpdateClientExePath()
        {
            GetEnvironmentProvider().GetUpdateClientExePath().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone.Update.exe");
        }

        [Test]
        public void GetSandboxLogFolder()
        {
            GetEnvironmentProvider().GetSandboxLogFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\UpdateLogs\");
        }

        [Test]
        public void GetUpdateLogFolder()
        {
            GetEnvironmentProvider().GetUpdateLogFolder().Should().BeEquivalentTo(@"C:\NzbDrone\UpdateLogs\");
        }
    }
}
