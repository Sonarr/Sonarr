using System.IO;
using AutoMoq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ConfigFileProviderTest : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            WithTempAsStartUpPath();

            //Reset config file
            var configFile = Mocker.Resolve<PathProvider>().AppConfigFile;

            if (File.Exists(configFile))
                File.Delete(configFile);

            Mocker.Resolve<ConfigFileProvider>().CreateDefaultConfigFile();
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

            var mocker = new AutoMoqer();

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().GetValueBoolean(key, value);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void GetLaunchBrowser_Success()
        {
            var mocker = new AutoMoqer();

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
        public void GetValue_New_Key_with_new_parent()
        {
            const string key = "Hello";
            const string value = "World";

            //Act
            var result = Mocker.Resolve<ConfigFileProvider>().GetValue(key, value, "Universe");

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
    }
}