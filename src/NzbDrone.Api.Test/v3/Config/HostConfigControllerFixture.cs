using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.Config;

namespace NzbDrone.Api.Test.v3.Config
{
    [TestFixture]
    public class HostConfigControllerFixture : TestBase<HostConfigControllerFixture.TestHostConfigController>
    {
        private const string PrivateValue = "********";

        private Dictionary<string, object> _savedConfigFile;
        private Dictionary<string, object> _savedConfigService;

        public class TestHostConfigController : HostConfigController
        {
            public TestHostConfigController(IConfigFileProvider configFileProvider,
                                            IConfigService configService,
                                            IUserService userService,
                                            IDiskProvider diskProvider,
                                            OidcAuthorityValidator oidcAuthorityValidator)
                : base(configFileProvider, configService, userService, diskProvider, oidcAuthorityValidator)
            {
            }

            public ValidationResult Validate(HostConfigResource resource) => SharedValidator.Validate(resource);
        }

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IUserService>()
                  .Setup(s => s.FindUser())
                  .Returns(new User { Username = "admin", Password = "stored-hash" });

            Mocker.GetMock<IConfigFileProvider>().SetupGet(s => s.OidcClientSecret).Returns("oidc-secret");
            Mocker.GetMock<IConfigFileProvider>().SetupGet(s => s.SslCertPassword).Returns("ssl-secret");
            Mocker.GetMock<IConfigService>().SetupGet(s => s.ProxyPassword).Returns("proxy-secret");

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.SaveConfigDictionary(It.IsAny<Dictionary<string, object>>()))
                  .Callback<Dictionary<string, object>>(d => _savedConfigFile = d);

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.SaveConfigDictionary(It.IsAny<Dictionary<string, object>>()))
                  .Callback<Dictionary<string, object>>(d => _savedConfigService = d);
        }

        private static HostConfigResource GivenResource(string password, string passwordConfirmation = "")
        {
            return new HostConfigResource
            {
                Id = 1,
                Username = "admin",
                Password = password,
                PasswordConfirmation = passwordConfirmation,
                OidcClientSecret = PrivateValue,
                SslCertPassword = PrivateValue,
                ProxyPassword = PrivateValue
            };
        }

        private IEnumerable<ValidationFailure> PasswordConfirmationErrors(HostConfigResource resource)
        {
            return Subject.Validate(resource).Errors.Where(e => e.PropertyName == nameof(HostConfigResource.PasswordConfirmation));
        }

        [Test]
        public void get_should_mask_secrets()
        {
            var resource = Subject.GetHostConfig();

            resource.Username.Should().Be("admin");
            resource.Password.Should().Be(PrivateValue);
            resource.OidcClientSecret.Should().Be(PrivateValue);
            resource.SslCertPassword.Should().Be(PrivateValue);
            resource.ProxyPassword.Should().Be(PrivateValue);
        }

        [Test]
        public void get_should_return_empty_secrets_when_not_set()
        {
            Mocker.GetMock<IUserService>().Setup(s => s.FindUser()).Returns((User)null);
            Mocker.GetMock<IConfigFileProvider>().SetupGet(s => s.OidcClientSecret).Returns(string.Empty);
            Mocker.GetMock<IConfigFileProvider>().SetupGet(s => s.SslCertPassword).Returns(string.Empty);
            Mocker.GetMock<IConfigService>().SetupGet(s => s.ProxyPassword).Returns(string.Empty);

            var resource = Subject.GetHostConfig();

            resource.Password.Should().BeEmpty();
            resource.OidcClientSecret.Should().BeEmpty();
            resource.SslCertPassword.Should().BeEmpty();
            resource.ProxyPassword.Should().BeEmpty();
        }

        [Test]
        public void save_should_keep_stored_secrets_when_masked()
        {
            Subject.SaveHostConfig(GivenResource(PrivateValue));

            _savedConfigFile["OidcClientSecret"].Should().Be("oidc-secret");
            _savedConfigFile["SslCertPassword"].Should().Be("ssl-secret");
            _savedConfigService["ProxyPassword"].Should().Be("proxy-secret");

            Mocker.GetMock<IUserService>().Verify(s => s.Upsert("admin", null), Times.Once());
        }

        [Test]
        public void save_should_store_new_secrets()
        {
            var resource = GivenResource("new-password", "new-password");
            resource.OidcClientSecret = "new-oidc";
            resource.SslCertPassword = "new-ssl";
            resource.ProxyPassword = "new-proxy";

            Subject.SaveHostConfig(resource);

            _savedConfigFile["OidcClientSecret"].Should().Be("new-oidc");
            _savedConfigFile["SslCertPassword"].Should().Be("new-ssl");
            _savedConfigService["ProxyPassword"].Should().Be("new-proxy");

            Mocker.GetMock<IUserService>().Verify(s => s.Upsert("admin", "new-password"), Times.Once());
        }

        [Test]
        public void save_should_store_password_matching_stored_hash_as_new_password()
        {
            Subject.SaveHostConfig(GivenResource("stored-hash", "stored-hash"));

            Mocker.GetMock<IUserService>().Verify(s => s.Upsert("admin", "stored-hash"), Times.Once());
        }

        [Test]
        public void validate_should_accept_masked_password_without_confirmation()
        {
            PasswordConfirmationErrors(GivenResource(PrivateValue)).Should().BeEmpty();
        }

        [Test]
        public void validate_should_accept_matching_confirmation()
        {
            PasswordConfirmationErrors(GivenResource("new-password", "new-password")).Should().BeEmpty();
        }

        [Test]
        public void validate_should_reject_mismatched_confirmation()
        {
            PasswordConfirmationErrors(GivenResource("new-password", "other")).Should().NotBeEmpty();
        }

        [Test]
        public void validate_should_reject_stored_hash_without_confirmation()
        {
            PasswordConfirmationErrors(GivenResource("stored-hash")).Should().NotBeEmpty();
        }
    }
}
