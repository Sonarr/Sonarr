using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class PathExtentionFixture
    {

        private EnviromentProvider GetEnviromentProvider()
        {
            var envMoq = new Mock<EnviromentProvider>();
            envMoq.SetupGet(c => c.ApplicationPath).Returns(@"C:\NzbDrone\");

            return envMoq.Object;
            }


        [Test]
        public void AppDataDirectory_path_test()
        {
            GetEnviromentProvider().GetAppDataPath().Should().BeEquivalentTo(@"C:\NzbDrone\NzbDrone.Web\App_Data\");
        }


        [Test]
        public void Config_path_test()
        {
            GetEnviromentProvider().GetConfigPath().Should().BeEquivalentTo(@"C:\NzbDrone\Config.xml");
        }

        [Test]
        public void IISConfig_path_test()
        {
            GetEnviromentProvider().GetIISConfigPath().Should().BeEquivalentTo(@"C:\NzbDrone\IISExpress\AppServer\applicationhost.config");
        }

        [Test]
        public void IISExe_path_test()
        {
            GetEnviromentProvider().GetIISExe().Should().BeEquivalentTo(@"C:\NzbDrone\IISExpress\IISExpress.exe");
        }

        [Test]
        public void NlogConfig_path_test()
        {
            GetEnviromentProvider().GetNlogConfigPath().Should().BeEquivalentTo(@"C:\NzbDrone\NzbDrone.Web\log.config");
        }
    }
}
