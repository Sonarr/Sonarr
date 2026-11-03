using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;
using Sonarr.Http.Authentication;

namespace Sonarr.Http.Test.Authentication
{
    [TestFixture]
    public class OidcTokenValidationServiceFixture : TestBase
    {
        private const string Authority = "https://auth.example.com";
        private const string ClientId = "test-client-id";
        private const string KeyFileName = "service-token.key";

        private Mock<IConfigFileProvider> _configMock;
        private Mock<IOptionsMonitor<IConfigFileProvider>> _configMonitor;
        private SymmetricSecurityKey _signingKey;
        private OpenIdConnectConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _configMock = new Mock<IConfigFileProvider>();
            _configMock.SetupGet(c => c.OidcAuthority).Returns(Authority);
            _configMock.SetupGet(c => c.OidcClientId).Returns(ClientId);

            _configMonitor = new Mock<IOptionsMonitor<IConfigFileProvider>>();
            _configMonitor.SetupGet(c => c.CurrentValue).Returns(_configMock.Object);

            // Register the mocked configuration so the subject under test sees the mocked
            // OidcAuthority/OidcClientId values instead of AutoMoq's unconfigured mock.
            Mocker.SetConstant<IConfigFileProvider>(_configMock.Object);
            Mocker.SetConstant<IOptionsMonitor<IConfigFileProvider>>(_configMonitor.Object);

            // The fixture owns its signing key, so create the key file TempFolder expects
            // before ReadKey() is called. TestBase only creates the directory.
            var keyBytes = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(TempFolder, KeyFileName),
                Convert.ToBase64String(keyBytes));

            _signingKey = new SymmetricSecurityKey(ReadKey()) { KeyId = "test-kid" };

            _configuration = new OpenIdConnectConfiguration { Issuer = Authority };
            _configuration.SigningKeys.Add(_signingKey);
        }

        private byte[] ReadKey()
        {
            var path = System.IO.Path.Combine(TempFolder, KeyFileName);
            var content = System.IO.File.ReadAllText(path).Trim();
            try
            {
                return Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                return Encoding.UTF8.GetBytes(content);
            }
        }

        private static IConfigurationManager<OpenIdConnectConfiguration> CreateManager(OpenIdConnectConfiguration configuration)
        {
            var mock = new Mock<IConfigurationManager<OpenIdConnectConfiguration>>();
            mock.Setup(manager => manager.GetConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(configuration);

            return mock.Object;
        }

        private OidcTokenValidationService CreateService(
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = null)
        {
            // Production DI passes only the options monitor and lazily builds the network-backed
            // manager; tests pass the manager too so discovery never hits the network while
            // OidcClientId still comes from config. No reflection is used.
            return new OidcTokenValidationService(configurationManager, _configMonitor.Object);
        }

        private string CreateToken(
            string issuer = Authority,
            string audience = ClientId,
            string subject = "subject-1",
            string username = "homies",
            IEnumerable<string> groups = null,
            int notBeforeMinutes = -1,
            int expiresMinutes = 60,
            SecurityKey signingKey = null)
        {
            var claims = new List<Claim>();

            if (subject != null)
            {
                claims.Add(new Claim("sub", subject));
            }

            if (username != null)
            {
                claims.Add(new Claim("preferred_username", username));
            }

            if (groups != null)
            {
                foreach (var group in groups)
                {
                    claims.Add(new Claim("groups", group));
                }
            }

            var credentials = new SigningCredentials(signingKey ?? _signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow.AddMinutes(notBeforeMinutes),
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [Test]
        public void should_accept_a_valid_oidc_token()
        {
            var token = CreateToken(groups: new[] { "homies", "family" });

            var result = CreateService(CreateManager(_configuration)).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(result.FailureReason, Is.Null);
            Assert.That(result.Subject, Is.EqualTo("subject-1"));
            Assert.That(result.Username, Is.EqualTo("homies"));
            Assert.That(result.Groups, Is.EquivalentTo(new[] { "homies", "family" }));
        }

        [Test]
        public void should_reject_expired_token()
        {
            var token = CreateToken(notBeforeMinutes: -20, expiresMinutes: -10);

            var result = CreateService(CreateManager(_configuration)).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo("invalid_token"));
        }

        [Test]
        public void should_reject_wrong_issuer()
        {
            var token = CreateToken(issuer: "https://evil.example.com");

            var result = CreateService(CreateManager(_configuration)).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo("invalid_token"));
        }

        [Test]
        public void should_reject_wrong_audience()
        {
            var token = CreateToken(audience: "wrong-client-id");

            var result = CreateService(CreateManager(_configuration)).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo("invalid_token"));
        }

        [Test]
        public void should_reject_missing_subject()
        {
            var token = CreateToken(subject: null);

            var result = CreateService(CreateManager(_configuration)).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo("missing_subject"));
        }

        [Test]
        public void should_handle_empty_audience_config_fails_closed()
        {
            _configMock.SetupGet(c => c.OidcClientId).Returns(string.Empty);
            var token = CreateToken();

            var result = CreateService(CreateManager(_configuration)).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo("invalid_token"));
        }

        [Test]
        public void should_handle_null_audience_config_fails_closed()
        {
            _configMock.SetupGet(c => c.OidcClientId).Returns((string)null);
            var token = CreateToken();

            var result = CreateService(CreateManager(_configuration)).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo("invalid_token"));
        }

        [Test]
        public void should_handle_missing_oidc_authority_fails_closed()
        {
            _configMock.SetupGet(c => c.OidcAuthority).Returns((string)null);
            var token = CreateToken();

            // No configuration manager is injected; the lazy path must fail closed before
            // any discovery request is attempted.
            var result = CreateService().ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo("invalid_token"));
        }

        [Test]
        public void should_accept_rotated_key_after_jwks_refresh()
        {
            var rotatedKeyBytes = Encoding.UTF8.GetBytes("rotated-key-0123456789abcdef-32b");
            var rotatedKey = new SymmetricSecurityKey(rotatedKeyBytes) { KeyId = "rotated-kid" };
            var rotatedConfiguration = new OpenIdConnectConfiguration { Issuer = Authority };
            rotatedConfiguration.SigningKeys.Add(rotatedKey);

            var manager = new Mock<IConfigurationManager<OpenIdConnectConfiguration>>();
            manager.SetupSequence(m => m.GetConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_configuration)
                .ReturnsAsync(rotatedConfiguration);

            var service = CreateService(manager.Object);
            var token = CreateToken(signingKey: rotatedKey);

            // The unknown kid triggers an in-call JWKS refresh and one retry against the
            // rotated key set, so the first exchange after rotation succeeds.
            var result = service.ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(result.Subject, Is.EqualTo("subject-1"));
            manager.Verify(m => m.RequestRefresh(), Times.Once);
            manager.Verify(m => m.GetConfigurationAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

            // If the refreshed key set is still stale the retry must yield a failure result,
            // never a thrown SecurityTokenException (C2).
            var staleManager = new Mock<IConfigurationManager<OpenIdConnectConfiguration>>();
            staleManager.SetupSequence(m => m.GetConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_configuration)
                .ReturnsAsync(_configuration);

            var staleResult = CreateService(staleManager.Object).ValidateAsync(token).GetAwaiter().GetResult();

            Assert.That(staleResult.Success, Is.False);
            Assert.That(staleResult.FailureReason, Is.EqualTo("invalid_token"));
            staleManager.Verify(m => m.RequestRefresh(), Times.Once);
        }
    }
}
