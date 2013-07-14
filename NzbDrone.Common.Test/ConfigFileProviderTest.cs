using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Model;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]

    public class ConfigFileProviderTest : TestBase<ConfigFileProvider>
    {
        [SetUp]
        public void SetUp()
        {
            WithTempAsAppPath();

            var configFile = Mocker.Resolve<IAppFolderInfo>().GetConfigPath();

            if (File.Exists(configFile))
                File.Delete(configFile);
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
            var result = Subject.AuthenticationEnabled;

            result.Should().Be(false);
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

    }
}