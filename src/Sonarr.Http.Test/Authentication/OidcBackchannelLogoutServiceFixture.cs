using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;
using Sonarr.Http.Authentication;

namespace Sonarr.Http.Test.Authentication
{
    [TestFixture]
    public class OidcBackchannelLogoutServiceFixture : TestBase<OidcBackchannelLogoutService>
    {
        private const string Issuer = "https://idp.example.com";
        private const string ClientId = "sonarr";
        private const string BackchannelLogoutEvent = "http://schemas.openid.net/event/backchannel-logout";

        private RsaSecurityKey _signingKey;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.AuthenticationMethod).Returns(AuthenticationType.Oidc);
            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.OidcAuthority).Returns(Issuer);
            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.OidcClientId).Returns(ClientId);
            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.OidcClientSecret).Returns("secret");
            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.OidcUserIdentifier).Returns("user@example.com");
            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.OidcScopes).Returns("openid email");

            _signingKey = new RsaSecurityKey(RSA.Create(2048)) { KeyId = "key1" };

            var configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
            configuration.SigningKeys.Add(_signingKey);

            var options = new OpenIdConnectOptions
            {
                ClientId = ClientId,
                Configuration = configuration
            };

            Mocker.GetMock<IOptionsMonitor<OpenIdConnectOptions>>()
                .Setup(c => c.Get(nameof(AuthenticationType.Oidc)))
                .Returns(options);
        }

        private Dictionary<string, object> GivenClaims()
        {
            return new Dictionary<string, object>
            {
                { "sub", "subject1" },
                { "sid", "session1" },
                { "jti", Guid.NewGuid().ToString() },
                { "events", new Dictionary<string, object> { { BackchannelLogoutEvent, new Dictionary<string, object>() } } }
            };
        }

        private string GivenLogoutToken(Dictionary<string, object> claims, string issuer = Issuer, string audience = ClientId, SecurityKey key = null)
        {
            return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(2),
                Claims = claims,
                TokenType = "logout+jwt",
                SigningCredentials = new SigningCredentials(key ?? _signingKey, SecurityAlgorithms.RsaSha256)
            });
        }

        private void VerifyNoLogout()
        {
            Mocker.GetMock<IOidcLogoutService>().Verify(c => c.LogoutSession(It.IsAny<string>()), Times.Never());
            Mocker.GetMock<IOidcLogoutService>().Verify(c => c.LogoutSubject(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task should_log_out_session_when_sid_is_provided()
        {
            var result = await Subject.LogoutAsync(GivenLogoutToken(GivenClaims()));

            result.Should().BeTrue();
            Mocker.GetMock<IOidcLogoutService>().Verify(c => c.LogoutSession("session1"), Times.Once());
            Mocker.GetMock<IOidcLogoutService>().Verify(c => c.LogoutSubject(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task should_log_out_subject_when_sid_is_not_provided()
        {
            var claims = GivenClaims();
            claims.Remove("sid");

            var result = await Subject.LogoutAsync(GivenLogoutToken(claims));

            result.Should().BeTrue();
            Mocker.GetMock<IOidcLogoutService>().Verify(c => c.LogoutSubject("subject1"), Times.Once());
            Mocker.GetMock<IOidcLogoutService>().Verify(c => c.LogoutSession(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_check_logout_using_principal_claims()
        {
            var issued = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new("sub", "subject1"), new("sid", "session1") }, "Cookies"));

            Mocker.GetMock<IOidcLogoutService>()
                .Setup(c => c.IsLoggedOut("session1", "subject1", new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)))
                .Returns(true);

            Subject.IsLoggedOut(principal, issued).Should().BeTrue();
        }

        [Test]
        public async Task should_reject_token_without_sid_or_sub()
        {
            var claims = GivenClaims();
            claims.Remove("sid");
            claims.Remove("sub");

            (await Subject.LogoutAsync(GivenLogoutToken(claims))).Should().BeFalse();
        }

        [Test]
        public async Task should_reject_token_without_logout_event()
        {
            var claims = GivenClaims();
            claims["events"] = new Dictionary<string, object> { { "http://example.com/other-event", new Dictionary<string, object>() } };

            (await Subject.LogoutAsync(GivenLogoutToken(claims))).Should().BeFalse();
            VerifyNoLogout();
        }

        [Test]
        public async Task should_reject_token_without_events_claim()
        {
            var claims = GivenClaims();
            claims.Remove("events");

            (await Subject.LogoutAsync(GivenLogoutToken(claims))).Should().BeFalse();
        }

        [Test]
        public async Task should_reject_token_with_nonce()
        {
            var claims = GivenClaims();
            claims["nonce"] = "abc";

            (await Subject.LogoutAsync(GivenLogoutToken(claims))).Should().BeFalse();
            VerifyNoLogout();
        }

        [Test]
        public async Task should_reject_token_with_wrong_issuer()
        {
            (await Subject.LogoutAsync(GivenLogoutToken(GivenClaims(), issuer: "https://other.example.com"))).Should().BeFalse();
        }

        [Test]
        public async Task should_reject_token_with_wrong_audience()
        {
            (await Subject.LogoutAsync(GivenLogoutToken(GivenClaims(), audience: "other"))).Should().BeFalse();
        }

        [Test]
        public async Task should_reject_token_signed_with_unknown_key()
        {
            var otherKey = new RsaSecurityKey(RSA.Create(2048)) { KeyId = "key2" };

            (await Subject.LogoutAsync(GivenLogoutToken(GivenClaims(), key: otherKey))).Should().BeFalse();
            VerifyNoLogout();
        }

        [Test]
        public async Task should_reject_malformed_token()
        {
            (await Subject.LogoutAsync("not-a-token")).Should().BeFalse();
        }

        [Test]
        public async Task should_reject_empty_token()
        {
            (await Subject.LogoutAsync(string.Empty)).Should().BeFalse();
        }

        [Test]
        public async Task should_reject_token_when_oidc_is_not_enabled()
        {
            Mocker.GetMock<IConfigFileProvider>().SetupGet(c => c.AuthenticationMethod).Returns(AuthenticationType.Forms);

            (await Subject.LogoutAsync(GivenLogoutToken(GivenClaims()))).Should().BeFalse();
        }
    }
}
