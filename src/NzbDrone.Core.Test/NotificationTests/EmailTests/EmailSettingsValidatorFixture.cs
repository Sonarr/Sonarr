using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Email;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests.EmailTests
{
    [TestFixture]
    public class EmailSettingsValidatorFixture : CoreTest<EmailSettingsValidator>
    {
        private EmailSettings _emailSettings;
        private TestValidator<EmailSettings> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new TestValidator<EmailSettings>
                            {
                                v => v.RuleFor(s => s).SetValidator(Subject)
                            };

            _emailSettings = Builder<EmailSettings>.CreateNew()
                                        .With(s => s.Server = "someserver")
                                        .With(s => s.Port = 567)
                                        .With(s => s.RequireEncryption = true)
                                        .With(s => s.From = "dont@email.me")
                                        .With(s => s.To = new string[] { "dont@email.me" })
                                        .Build();
        }

        [Test]
        public void should_be_valid_if_all_settings_valid()
        {
            _validator.Validate(_emailSettings).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_not_be_valid_if_port_is_out_of_range()
        {
            _emailSettings.Port = 900000;

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_server_is_empty()
        {
            _emailSettings.Server = "";

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_from_is_empty()
        {
            _emailSettings.From = "";

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }

        [TestCase("sonarr")]
        [TestCase("sonarr@sonarr")]
        [TestCase("email.me")]
        [Ignore("Allowed coz some email servers allow arbitrary source, we probably need to support 'Name <email>' syntax")]
        public void should_not_be_valid_if_from_is_invalid(string email)
        {
            _emailSettings.From = email;

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }

        [TestCase("sonarr")]
        [TestCase("sonarr@sonarr")]
        [TestCase("email.me")]
        public void should_not_be_valid_if_to_is_invalid(string email)
        {
            _emailSettings.To = new string[] { email };

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }

        [TestCase("sonarr")]
        [TestCase("sonarr@sonarr")]
        [TestCase("email.me")]
        public void should_not_be_valid_if_cc_is_invalid(string email)
        {
            _emailSettings.Cc = new string[] { email };

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }

        [TestCase("sonarr")]
        [TestCase("sonarr@sonarr")]
        [TestCase("email.me")]
        public void should_not_be_valid_if_bcc_is_invalid(string email)
        {
            _emailSettings.Bcc = new string[] { email };

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_to_bcc_cc_are_all_empty()
        {
            _emailSettings.To = new string[] { };
            _emailSettings.Cc = new string[] { };
            _emailSettings.Bcc = new string[] { };

            _validator.Validate(_emailSettings).IsValid.Should().BeFalse();
        }
    }
}
