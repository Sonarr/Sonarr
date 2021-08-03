using NUnit.Framework;
using NzbDrone.Core.Notifications.Prowl;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.NotificationTests
{
    [Explicit]
    [ManualTest]
    [TestFixture]
    public class ProwlProviderTest : CoreTest<ProwlProxy>
    {
        private const string _apiKey = "66e9f688b512152eb2688f0486ae542c76e564a2";

        private const string _badApiKey = "1234567890abcdefghijklmnopqrstuvwxyz1234";

        private ProwlSettings _settings = new ProwlSettings { ApiKey = _apiKey };

        [Test]
        public void Verify_should_not_throw_for_a_valid_apiKey()
        {
            Subject.Test(_settings);
            ExceptionVerification.ExpectedWarns(0);
        }

        [Test]
        public void Verify_should_throw_for_an_invalid_apiKey()
        {
            _settings.ApiKey = _badApiKey;

            Assert.Throws<ProwlException>(() => Subject.Test(_settings));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void SendNotification_should_not_throw_for_a_valid_apiKey()
        {
            Subject.SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _settings);
            ExceptionVerification.ExpectedWarns(0);
        }

        [Test]
        public void SendNotification_should_log_a_warning_for_an_invalid_apiKey()
        {
            _settings.ApiKey = _badApiKey;

            Subject.SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _settings);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
