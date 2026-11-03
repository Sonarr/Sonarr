using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;
using Sonarr.Http.Authentication;

namespace Sonarr.Http.Test.Authentication
{
    [TestFixture]
    public class AuthenticationControllerOidcFixture : TestBase<AuthenticationController>
    {
        private static int _clientIpCounter;

        private AuthenticationController _controller;
        private DefaultHttpContext _httpContext;
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IConfigFileProvider> _configMock;
        private Mock<IOidcTokenValidationService> _tokenValidationMock;
        private Mock<ISonarrServiceTokenService> _tokenServiceMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IUserRepository> _userRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthenticationService>();
            _configMock = new Mock<IConfigFileProvider>();
            _tokenValidationMock = new Mock<IOidcTokenValidationService>();
            _tokenServiceMock = new Mock<ISonarrServiceTokenService>();
            _userServiceMock = new Mock<IUserService>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _configMock.SetupGet(c => c.OidcAuthority).Returns("https://auth.example.com");
            _configMock.SetupGet(c => c.OidcClientId).Returns("test-client-id");
            _configMock.SetupGet(c => c.OidcRoleMappings).Returns("homies=ReadOnly;admins=Admin");
            _configMock.SetupGet(c => c.UrlBase).Returns(string.Empty);

            // Register the mocked configuration so any Mocker-resolved dependency sees the
            // mocked OidcAuthority/OidcClientId/OidcRoleMappings values.
            Mocker.SetConstant<IConfigFileProvider>(_configMock.Object);

            // Login has three parameters (HttpRequest, username, password); the callback must
            // supply all three or Moq fails to bind it.
            _authServiceMock.Setup(m => m.Login(It.IsAny<HttpRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((HttpRequest request, string username, string password) =>
                    username == "testuser" && password == "password"
                        ? new User { Id = 1, Identifier = Guid.NewGuid(), Username = "testuser", Role = SonarrRole.ReadOnly }
                        : null);

            _authServiceMock.Setup(m => m.GetLockoutEndTime(It.IsAny<HttpRequest>())).Returns((DateTime?)null);
            _authServiceMock.Setup(m => m.Logout(It.IsAny<HttpContext>()));

            _httpContext = new DefaultHttpContext();
            _httpContext.Connection.RemoteIpAddress = NextClientIp();

            // The controller resolves the OIDC validator, token service and user services from
            // HttpContext.RequestServices, not from its constructor.
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(_tokenValidationMock.Object);
            services.AddSingleton(_tokenServiceMock.Object);
            services.AddSingleton(_userServiceMock.Object);
            services.AddSingleton(_userRepositoryMock.Object);
            _httpContext.RequestServices = services.BuildServiceProvider();

            _controller = new AuthenticationController(
                _authServiceMock.Object,
                _configMock.Object,
                Mocker.Resolve<IAppFolderInfo>(),
                TestLogger);

            _controller.ControllerContext = new ControllerContext { HttpContext = _httpContext };
        }

        private static IPAddress NextClientIp()
        {
            // Each test uses its own client IP so the static exchange rate limiter (keyed by
            // IP, one-minute window) cannot leak state between tests.
            var value = Interlocked.Increment(ref _clientIpCounter) % 250;
            return IPAddress.Parse($"198.51.100.{value + 1}");
        }

        private static OidcExchangeRequest CreateOidcExchange(string token) => new(token, null);

        private void SetupValidation(string token, bool success, params string[] groups)
        {
            _tokenValidationMock
                .Setup(v => v.ValidateAsync(token))
                .ReturnsAsync(new OidcValidationResult(
                    success,
                    success ? null : "invalid_token",
                    success ? "subject-1" : null,
                    success ? "alice" : null,
                    groups.ToList()));
        }

        private async Task<(int StatusCode, string Body)> ExecuteAsync(IResult result)
        {
            _httpContext.Response.Body = new MemoryStream();

            await result.ExecuteAsync(_httpContext);

            _httpContext.Response.Body.Position = 0;
            using var reader = new StreamReader(_httpContext.Response.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();

            return (_httpContext.Response.StatusCode, body);
        }

        private async Task<(int StatusCode, string Body)> ExchangeAsync(string token)
        {
            return await ExecuteAsync(await _controller.ExchangeOidc(CreateOidcExchange(token)));
        }

        [Test]
        public async Task exchange_with_valid_token_returns_200_with_token_shape()
        {
            SetupValidation("good-token", true, "homies");

            _userServiceMock
                .Setup(u => u.UpsertOidcUser("subject-1", "alice", "homies", SonarrRole.ReadOnly))
                .Returns(new User { Username = "alice" });

            _tokenServiceMock
                .Setup(t => t.MintToken("subject-1", SonarrRole.ReadOnly))
                .Returns("sonarr-token");

            var (status, body) = await ExchangeAsync("good-token");

            status.Should().Be(200);

            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;

            root.GetProperty("token").GetString().Should().Be("sonarr-token");
            root.GetProperty("token_type").GetString().Should().Be("Bearer");
            root.GetProperty("expires_in").GetInt32().Should().Be(300);
            root.GetProperty("role").GetString().Should().Be("ReadOnly");
            root.TryGetProperty("permissions", out var permissions).Should().BeTrue();
            permissions.ValueKind.Should().Be(JsonValueKind.Array);

            _userServiceMock.Verify(
                u => u.UpsertOidcUser("subject-1", "alice", "homies", SonarrRole.ReadOnly),
                Times.Once);
            _tokenServiceMock.Verify(t => t.MintToken("subject-1", SonarrRole.ReadOnly), Times.Once);
        }

        [Test]
        public async Task exchange_with_null_token_returns_400()
        {
            var (status, body) = await ExchangeAsync(null);

            status.Should().Be(400);
            body.Should().Contain("subject_token is required");

            _tokenValidationMock.Verify(v => v.ValidateAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task exchange_with_whitespace_token_returns_400()
        {
            var (status, body) = await ExchangeAsync("   ");

            status.Should().Be(400);
            body.Should().Contain("subject_token is required");
        }

        [Test]
        public async Task exchange_with_invalid_token_returns_401()
        {
            SetupValidation("bad-token", false);

            var (status, body) = await ExchangeAsync("bad-token");

            status.Should().Be(401);
            body.Should().Contain("invalid_token");

            _userServiceMock.Verify(
                u => u.UpsertOidcUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SonarrRole>()),
                Times.Never);
        }

        [Test]
        public async Task exchange_with_unmapped_group_returns_403()
        {
            SetupValidation("unmapped-token", true, "strangers");

            var (status, body) = await ExchangeAsync("unmapped-token");

            status.Should().Be(403);
            body.Should().Contain("insufficient_group");

            _userServiceMock.Verify(
                u => u.UpsertOidcUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SonarrRole>()),
                Times.Never);
            _tokenServiceMock.Verify(
                t => t.MintToken(It.IsAny<string>(), It.IsAny<SonarrRole>()),
                Times.Never);
        }

        [Test]
        public async Task exchange_rate_limit_returns_429_on_eleventh_request()
        {
            _httpContext.Connection.RemoteIpAddress = NextClientIp();

            _tokenValidationMock
                .Setup(v => v.ValidateAsync(It.IsAny<string>()))
                .ReturnsAsync(new OidcValidationResult(true, null, "subject-1", "alice", new List<string> { "homies" }));

            _userServiceMock
                .Setup(u => u.UpsertOidcUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SonarrRole>()))
                .Returns(new User { Username = "alice" });

            _tokenServiceMock
                .Setup(t => t.MintToken(It.IsAny<string>(), It.IsAny<SonarrRole>()))
                .Returns("sonarr-token");

            for (var i = 0; i < 10; i++)
            {
                var (status, _) = await ExchangeAsync("good-token-" + i);
                status.Should().Be(200);
            }

            var (limitedStatus, limitedBody) = await ExchangeAsync("good-token-11");

            limitedStatus.Should().Be(429);
            limitedBody.Should().Contain("rate_limit_exceeded");
            _tokenValidationMock.Verify(v => v.ValidateAsync(It.IsAny<string>()), Times.Exactly(10));
        }

        [Test]
        public async Task me_without_token_returns_401()
        {
            var (status, body) = await ExecuteAsync(_controller.GetMe());

            status.Should().Be(401);
            body.Should().Contain("invalid_token");
        }

        [Test]
        public async Task me_with_valid_token_but_unknown_subject_returns_404()
        {
            _httpContext.Request.Headers.Authorization = "Bearer valid-token";

            _tokenServiceMock
                .Setup(t => t.ValidateToken("valid-token"))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "subject-1") }, "test")));

            _userRepositoryMock
                .Setup(r => r.FindBySubject("subject-1"))
                .Returns((User)null);

            var (status, body) = await ExecuteAsync(_controller.GetMe());

            status.Should().Be(404);
            body.Should().Contain("not_found");
        }

        [Test]
        public async Task me_with_valid_token_returns_200_with_user_shape()
        {
            _httpContext.Request.Headers.Authorization = "Bearer valid-token";

            _tokenServiceMock
                .Setup(t => t.ValidateToken("valid-token"))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "subject-1") }, "test")));

            _userRepositoryMock
                .Setup(r => r.FindBySubject("subject-1"))
                .Returns(new User { Username = "alice", OidcGroups = "homies", Role = SonarrRole.ReadOnly });

            var (status, body) = await ExecuteAsync(_controller.GetMe());

            status.Should().Be(200);

            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;

            root.GetProperty("username").GetString().Should().Be("alice");
            root.GetProperty("role").GetString().Should().Be("ReadOnly");
            root.GetProperty("groups").EnumerateArray().Select(g => g.GetString()).Should().BeEquivalentTo("homies");
            root.TryGetProperty("permissions", out var permissions).Should().BeTrue();
            permissions.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }
}
