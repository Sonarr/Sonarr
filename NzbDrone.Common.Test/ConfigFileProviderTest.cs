using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Model;
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

            //Reset config file
            var configFile = Mocker.Resolve<IEnvironmentProvider>().GetConfigPath();

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
            var result = Subject.AuthenticationType;

            result.Should().Be(AuthenticationType.Anonymous);
        }

        [Test]
        public void GetAuthenticationType_Basic()
        {
            Subject.SetValue("AuthenticationType", AuthenticationType.Basic);
            
            var result = Subject.AuthenticationType;
            
            result.Should().Be(AuthenticationType.Basic);
        }

        [Test]
        public void Guid_should_return_the_same_every_time()
        {

            var firstResponse = Subject.Guid;
            var secondResponse = Subject.Guid;



            secondResponse.Should().Be(firstResponse);
        }
    }
}