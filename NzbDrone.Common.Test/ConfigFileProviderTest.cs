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
    // ReSharper disable InconsistentNaming
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

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().GetValue(key, value);

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void GetInt_Success()
        {
            const string key = "Port";
            const int value = 8989;

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().GetValueInt(key, value);

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void GetBool_Success()
        {
            const string key = "LaunchBrowser";
            const bool value = true;

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().GetValueBoolean(key, value);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void GetLaunchBrowser_Success()
        {
            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().LaunchBrowser;

            //Assert
            result.Should().Be(true);
        }

        [Test]
        public void GetPort_Success()
        {
            const int value = 8989;

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().Port;

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void SetValue_bool()
        {
            const string key = "LaunchBrowser";
            const bool value = false;

            //Act
            Mocker.Resolve<ConfigFileProvider>().SetValue(key, value);

            //Assert
            var result = Mocker.Resolve<ConfigFileProvider>().LaunchBrowser;
            result.Should().Be(value);
        }

        [Test]
        public void SetValue_int()
        {
            const string key = "Port";
            const int value = 12345;

            //Act
            Mocker.Resolve<ConfigFileProvider>().SetValue(key, value);

            //Assert
            var result = Mocker.Resolve<ConfigFileProvider>().Port;
            result.Should().Be(value);
        }

        [Test]
        public void GetValue_New_Key()
        {
            const string key = "Hello";
            const string value = "World";

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().GetValue(key, value);

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void GetAuthenticationType_No_Existing_Value()
        {

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().AuthenticationType;

            //Assert
            result.Should().Be(AuthenticationType.Anonymous);
        }

        [Test]
        public void GetAuthenticationType_Windows()
        {

            Mocker.Resolve<ConfigFileProvider>().SetValue("AuthenticationType", 1);

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().AuthenticationType;

            //Assert
            result.Should().Be(AuthenticationType.Windows);
        }

        [Test]
        public void Guid_should_return_the_same_every_time()
        {
            //Act
            var firstResponse = Mocker.Resolve<ConfigFileProvider>().Guid;
            var secondResponse = Mocker.Resolve<ConfigFileProvider>().Guid;


            //Assert
            secondResponse.Should().Be(firstResponse);
        }
    }
}