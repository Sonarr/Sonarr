using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Providers;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ConfigProviderTest
    {

        private ConfigProvider GetConfigProvider()
        {
            var envMoq = new Mock<EnviromentProvider>();
            envMoq.SetupGet(c => c.ApplicationPath).Returns(@"C:\NzbDrone\");

            return new ConfigProvider(envMoq.Object);
        }


        [Test]
        public void IISExpress_path_test()
        {
            GetConfigProvider().IISDirectory.Should().BeEquivalentTo(@"C:\NzbDrone\IISExpress");
        }

        [Test]
        public void AppDataDirectory_path_test()
        {
            GetConfigProvider().AppDataDirectory.Should().BeEquivalentTo(@"C:\NzbDrone\NzbDrone.Web\App_Data");
        }


        [Test]
        public void Config_path_test()
        {
            GetConfigProvider().ConfigFile.Should().BeEquivalentTo(@"C:\NzbDrone\NzbDrone.Web\App_Data\Config.xml");
        }

        [Test]
        public void IISConfig_path_test()
        {
            GetConfigProvider().IISConfigPath.Should().BeEquivalentTo(@"C:\NzbDrone\IISExpress\AppServer\applicationhost.config");
        }

        [Test]
        public void IISExe_path_test()
        {
            GetConfigProvider().IISExePath.Should().BeEquivalentTo(@"C:\NzbDrone\IISExpress\IISExpress.exe");
        }

        [Test]
        public void NlogConfig_path_test()
        {
            GetConfigProvider().NlogConfigPath.Should().BeEquivalentTo(@"C:\NzbDrone\NzbDrone.Web\log.config");
        }
    }
}
