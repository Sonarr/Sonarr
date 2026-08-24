using Moq;
using NUnit.Framework;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class AllowedHostsCheckFixture : CoreTest<AllowedHostsCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");

            GivenAllowedHosts(string.Empty);
            GivenAuthenticationRequired(AuthenticationRequiredType.Enabled);
        }

        private void GivenAllowedHosts(string allowedHosts)
        {
            Mocker.GetMock<IConfigFileProvider>()
                  .SetupGet(s => s.AllowedHosts)
                  .Returns(allowedHosts);
        }

        private void GivenAuthenticationRequired(AuthenticationRequiredType required)
        {
            Mocker.GetMock<IConfigFileProvider>()
                  .SetupGet(s => s.AuthenticationRequired)
                  .Returns(required);
        }

        [Test]
        public void should_return_ok_when_authentication_is_required()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_when_authentication_is_not_required_for_local_addresses()
        {
            GivenAuthenticationRequired(AuthenticationRequiredType.DisabledForLocalAddresses);

            Subject.Check().ShouldBeWarning();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_ok_when_allowed_hosts_is_configured()
        {
            GivenAllowedHosts("sonarr.local");
            GivenAuthenticationRequired(AuthenticationRequiredType.DisabledForLocalAddresses);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_when_allowed_hosts_is_whitespace()
        {
            GivenAllowedHosts(" , ");
            GivenAuthenticationRequired(AuthenticationRequiredType.DisabledForLocalAddresses);

            Subject.Check().ShouldBeWarning();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
