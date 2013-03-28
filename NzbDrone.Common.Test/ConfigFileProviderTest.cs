using System.Linq;
using System.IO;

using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Model;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    
    public class ConfigFileProviderTest : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            WithTempAsAppPath();

            //Reset config file
            var configFile = Mocker.Resolve<EnvironmentProvider>().GetConfigPath();

            if (File.Exists(configFile))
                File.Delete(configFile);
        }

        [Test]
        public void GetValue_Success()
        {
            const string key = "Port";
            const string value = "8989";

            
            var result = Mocker.Resolve<ConfigFileProvider>().GetValue(key, value);

            
            result.Should().Be(value);
        }

        [Test]
        public void GetInt_Success()
        {
            const string key = "Port";
            const int value = 8989;

            
            var result = Mocker.Resolve<ConfigFileProvider>().GetValueInt(key, value);

            
            result.Should().Be(value);
        }

        [Test]
        public void GetBool_Success()
        {
            const string key = "LaunchBrowser";
            const bool value = true;

            
            var result = Mocker.Resolve<ConfigFileProvider>().GetValueBoolean(key, value);

            
            result.Should().BeTrue();
        }

        [Test]
        public void GetLaunchBrowser_Success()
        {
            
            var result = Mocker.Resolve<ConfigFileProvider>().LaunchBrowser;

            
            result.Should().Be(true);
        }

        [Test]
        public void GetPort_Success()
        {
            const int value = 8989;

            
            var result = Mocker.Resolve<ConfigFileProvider>().Port;

            
            result.Should().Be(value);
        }

        [Test]
        public void SetValue_bool()
        {
            const string key = "LaunchBrowser";
            const bool value = false;

            
            Mocker.Resolve<ConfigFileProvider>().SetValue(key, value);

            
            var result = Mocker.Resolve<ConfigFileProvider>().LaunchBrowser;
            result.Should().Be(value);
        }

        [Test]
        public void SetValue_int()
        {
            const string key = "Port";
            const int value = 12345;

            
            Mocker.Resolve<ConfigFileProvider>().SetValue(key, value);

            
            var result = Mocker.Resolve<ConfigFileProvider>().Port;
            result.Should().Be(value);
        }

        [Test]
        public void GetValue_New_Key()
        {
            const string key = "Hello";
            const string value = "World";

            
            var result = Mocker.Resolve<ConfigFileProvider>().GetValue(key, value);

            
            result.Should().Be(value);
        }

        [Test]
        public void GetAuthenticationType_No_Existing_Value()
        {

            
            var result = Mocker.Resolve<ConfigFileProvider>().AuthenticationType;

            
            result.Should().Be(AuthenticationType.Anonymous);
        }

        [Test]
        public void GetAuthenticationType_Windows()
        {

            Mocker.Resolve<ConfigFileProvider>().SetValue("AuthenticationType", 1);

            
            var result = Mocker.Resolve<ConfigFileProvider>().AuthenticationType;

            
            result.Should().Be(AuthenticationType.Windows);
        }

        [Test]
        public void Guid_should_return_the_same_every_time()
        {
            
            var firstResponse = Mocker.Resolve<ConfigFileProvider>().Guid;
            var secondResponse = Mocker.Resolve<ConfigFileProvider>().Guid;


            
            secondResponse.Should().Be(firstResponse);
        }
    }
}