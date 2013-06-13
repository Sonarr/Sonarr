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
    public class ProwlProviderTest : CoreTest<ProwlService>
    {
        private const string _apiKey = "66e9f688b512152eb2688f0486ae542c76e564a2";

        private const string _badApiKey = "1234567890abcdefghijklmnopqrstuvwxyz1234";

        [Test]
        public void Verify_should_not_throw_for_a_valid_apiKey()
        {
            Subject.Verify(_apiKey);
            ExceptionVerification.ExpectedWarns(0);
        }

        [Test]
        public void Verify_should_throw_for_an_invalid_apiKey()
        {
            Assert.Throws<InvalidApiKeyException>(() => Subject.Verify(_badApiKey));
            
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void SendNotification_should_not_throw_for_a_valid_apiKey()
        {
            Subject.SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey);
            ExceptionVerification.ExpectedWarns(0);
        }

        [Test]
        public void SendNotification_should_log_a_warning_for_an_invalid_apiKey()
        {
            Subject.SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _badApiKey);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}