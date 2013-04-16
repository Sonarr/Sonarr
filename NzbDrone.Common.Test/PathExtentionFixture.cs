using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class PathExtentionFixture : TestBase
    {

        private EnvironmentProvider GetEnviromentProvider()
        {
            var envMoq = new Mock<EnvironmentProvider>();

            envMoq.SetupGet(c => c.WorkingDirectory).Returns(@"C:\NzbDrone\");

            envMoq.SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");

            return envMoq.Object;
        }

        [TestCase(@"c:\test\", @"c:\test")]
        [TestCase(@"c:\\test\\", @"c:\test")]
        [TestCase(@"C:\\Test\\", @"C:\Test")]
        [TestCase(@"C:\\Test\\Test\", @"C:\Test\Test")]
        [TestCase(@"\\Testserver\Test\", @"\\Testserver\Test")]
        public void Normalize_Path(string dirty, string clean)
        {
            var result = dirty.NormalizePath();
            result.Should().Be(clean);
        }

        [Test]
        public void normalize_path_exception_empty()
        {
            Assert.Throws<ArgumentException>(()=> "".NormalizePath());
            ExceptionVerification.ExpectedWarns(1);


        }

        [Test]
        public void normalize_path_exception_null()
        {
            string nullPath = null;
            Assert.Throws<ArgumentException>(() => nullPath.NormalizePath());
            ExceptionVerification.ExpectedWarns(1);
        }


        [Test]
        public void AppDataDirectory_path_test()
        {
            GetEnviromentProvider().GetAppDataPath().Should().BeEquivalentTo(@"C:\NzbDrone\App_Data\");
        }


        [Test]
        public void Config_path_test()
        {
            GetEnviromentProvider().GetConfigPath().Should().BeEquivalentTo(@"C:\NzbDrone\Config.xml");
        }

        [Test]
        public void Sanbox()
        {
            GetEnviromentProvider().GetUpdateSandboxFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\");
        }

        [Test]
        public void GetUpdatePackageFolder()
        {
            GetEnviromentProvider().GetUpdatePackageFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\");
        }

        [Test]
        public void GetUpdateClientFolder()
        {
            GetEnviromentProvider().GetUpdateClientFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone\NzbDrone.Update\");
        }

        [Test]
        public void GetUpdateClientExePath()
        {
            GetEnviromentProvider().GetUpdateClientExePath().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\NzbDrone.Update.exe");
        }

        [Test]
        public void GetSandboxLogFolder()
        {
            GetEnviromentProvider().GetSandboxLogFolder().Should().BeEquivalentTo(@"C:\Temp\Nzbdrone_update\UpdateLogs\");
        }

        [Test]
        public void GetUpdateLogFolder()
        {
            GetEnviromentProvider().GetUpdateLogFolder().Should().BeEquivalentTo(@"C:\NzbDrone\UpdateLogs\");
        }
    }
}
