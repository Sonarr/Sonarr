using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Prowl;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using Prowlin;

namespace NzbDrone.Core.Test.NotificationTests
{
    [Explicit]
    [TestFixture]
    public class ProwlProviderTest : CoreTest
    {
        private const string _apiKey = "c3bdc0f48168f72d546cc6872925b160f5cbffc1";
        private const string _apiKey2 = "46a710a46b111b0b8633819b0d8a1e0272a3affa";

        private const string _badApiKey = "1234567890abcdefghijklmnopqrstuvwxyz1234";

        [Test]
        public void Verify_should_return_true_for_a_valid_apiKey()
        {
            
            
            
            
            var result = Mocker.Resolve<ProwlProvider>().Verify(_apiKey);

            
            result.Should().BeTrue();
        }

        [Test]
        public void Verify_should_return_false_for_an_invalid_apiKey()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().Verify(_badApiKey);

            
            ExceptionVerification.ExpectedWarns(1);
            result.Should().BeFalse();
        }

        [Test]
        public void SendNotification_should_return_true_for_a_valid_apiKey()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey);

            
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_return_false_for_an_invalid_apiKey()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _badApiKey);

            
            ExceptionVerification.ExpectedWarns(1);
            result.Should().BeFalse();
        }

        [Test]
        public void SendNotification_should_alert_with_high_priority()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone (High)", _apiKey, NotificationPriority.High);

            
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_alert_with_VeryLow_priority()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone (VeryLow)", _apiKey, NotificationPriority.VeryLow);

            
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_have_a_call_back_url()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey, NotificationPriority.Normal, "http://www.nzbdrone.com");

            
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_return_true_for_two_valid_apiKey()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey + ", " + _apiKey2);

            
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_return_true_for_valid_apiKey_with_bad_apiKey()
        {
            
            

            
            var result = Mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey + ", " + _badApiKey);

            
            result.Should().BeTrue();
        }
    }
}