using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]

    public class ConfigFileProviderTest : TestBase<ConfigFileProvider>
    {
        private string _configFileContents;

        [SetUp]
        public void SetUp()
        {
            WithTempAsAppPath();

            var configFile = Mocker.Resolve<IAppFolderInfo>().GetConfigPath();

            _configFileContents = null;

            WithMockConfigFile(configFile);
        }

        protected void WithMockConfigFile(string configFile)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FileExists(configFile))
                .Returns<string>(p => _configFileContents != null);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.ReadAllText(configFile))
                .Returns<string>(p => _configFileContents);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.WriteAllText(configFile, It.IsAny<string>()))
                .Callback<string, string>((p, t) => _configFileContents = t);
        }

        [Test]
        public void GetValue_Success()
        {
            const string key = "Port";
            const string value = "8989";

            var result = Subject.GetValue(key, value);

            result.Should().Be(value);
        }

        [Test]
        public void GetInt_Success()
        {
            const string key = "Port";
            const int value = 8989;


            var result = Subject.GetValueInt(key, value);


            result.Should().Be(value);
        }

        [Test]
        public void GetBool_Success()
        {
            const string key = "LaunchBrowser";
            const bool value = true;


            var result = Subject.GetValueBoolean(key, value);


            result.Should().BeTrue();
        }

        [Test]
        public void GetLaunchBrowser_Success()
        {

            var result = Subject.LaunchBrowser;


            result.Should().Be(true);
        }

        [Test]
        public void GetPort_Success()
        {
            const int value = 8989;


            var result = Subject.Port;


            result.Should().Be(value);
        }

        [Test]
        public void SetValue_bool()
        {
            const string key = "LaunchBrowser";
            const bool value = false;


            Subject.SetValue(key, value);


            var result = Subject.LaunchBrowser;
            result.Should().Be(value);
        }

        [Test]
        public void SetValue_int()
        {
            const string key = "Port";
            const int value = 12345;


            Subject.SetValue(key, value);


            var result = Subject.Port;
            result.Should().Be(value);
        }

        [Test]
        public void GetValue_New_Key()
        {
            const string key = "Hello";
            const string value = "World";


            var result = Subject.GetValue(key, value);


            result.Should().Be(value);
        }

        [Test]
        public void GetAuthenticationType_No_Existing_Value()
        {
            var result = Subject.AuthenticationMethod;

            result.Should().Be(AuthenticationType.None);
        }

        [Test]
        public void SaveDictionary_should_save_proper_value()
        {
            int port = 20555;

            var dic = Subject.GetConfigDictionary();
            dic["Port"] = 20555;

            Subject.SaveConfigDictionary(dic);

            Subject.Port.Should().Be(port);
        }

        [Test]
        public void SaveDictionary_should_only_save_specified_values()
        {
            int port = 20555;
            int origSslPort = 20551;
            int sslPort = 20552;

            var dic = Subject.GetConfigDictionary();
            dic["Port"] = port;
            dic["SslPort"] = origSslPort;
            Subject.SaveConfigDictionary(dic);


            dic = new Dictionary<string, object>();
            dic["SslPort"] = sslPort;
            Subject.SaveConfigDictionary(dic);

            Subject.Port.Should().Be(port);
            Subject.SslPort.Should().Be(sslPort);
        }

    }
}