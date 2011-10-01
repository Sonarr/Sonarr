using System.IO;
using AutoMoq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ConfigFileProviderTest : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            //Reset config file
            var mocker = new AutoMoqer();
            var configFile = mocker.Resolve<ConfigFileProvider>().ConfigFile;
            File.Delete(configFile);

            mocker.Resolve<ConfigFileProvider>().CreateDefaultConfigFile();
        }

        [Test]
        public void GetValue_Success()
        {
            const string key = "Port";
            const string value = "8989";

            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<ConfigFileProvider>().GetValue(key);

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void GetInt_Success()
        {
            const string key = "Port";
            const int value = 8989;

            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<ConfigFileProvider>().GetValueInt(key);

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void GetBool_Success()
        {
            const string key = "LaunchBrowser";

            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<ConfigFileProvider>().GetValueBoolean(key);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void GetLaunchBrowser_Success()
        {
            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<ConfigFileProvider>().LaunchBrowser;

            //Assert
            result.Should().Be(true);
        }

        [Test]
        public void GetPort_Success()
        {
            const int value = 8989;

            var mocker = new AutoMoqer();

            //Act
            var result = mocker.Resolve<ConfigFileProvider>().Port;

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void SetValue_bool()
        {
            const string key = "LaunchBrowser";
            const bool value = false;

            var mocker = new AutoMoqer();

            //Act
            mocker.Resolve<ConfigFileProvider>().SetValue(key, value);

            //Assert
            var result = mocker.Resolve<ConfigFileProvider>().LaunchBrowser;
            result.Should().Be(value);
        }

        [Test]
        public void SetValue_int()
        {
            const string key = "Port";
            const int value = 12345;

            var mocker = new AutoMoqer();

            //Act
            mocker.Resolve<ConfigFileProvider>().SetValue(key, value);

            //Assert
            var result = mocker.Resolve<ConfigFileProvider>().Port;
            result.Should().Be(value);
        }
    }
}