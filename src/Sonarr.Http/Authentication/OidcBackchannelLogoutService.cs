using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Authentication
{
    public interface IOidcBackchannelLogoutService
    {
        Task<bool> LogoutAsync(string logoutToken, CancellationToken cancellationToken = default);
        bool IsLoggedOut(ClaimsPrincipal principal, DateTimeOffset? issuedUtc);
    }

    public class OidcBackchannelLogoutService : IOidcBackchannelLogoutService
    {
        private const string BackchannelLogoutEvent = "http://schemas.openid.net/event/backchannel-logout";

        private readonly IConfigFileProvider _configFileProvider;
        private readonly IOptionsMonitor<OpenIdConnectOptions> _oidcOptions;
        private readonly IOidcLogoutService _oidcLogoutService;
        private readonly Logger _logger;

        public OidcBackchannelLogoutService(IConfigFileProvider configFileProvider,
                                            IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
                                            IOidcLogoutService oidcLogoutService,
                                            Logger logger)
        {
            _configFileProvider = configFileProvider;
            _oidcOptions = oidcOptions;
            _oidcLogoutService = oidcLogoutService;
            _logger = logger;
        }

        public async Task<bool> LogoutAsync(string logoutToken, CancellationToken cancellationToken = default)
        {
            if (_configFileProvider.EffectiveAuthenticationMethod() != AuthenticationType.Oidc ||
                !_configFileProvider.IsOidcConfigured())
            {
                _logger.Debug("Ignoring OIDC back-channel logout request, OIDC authentication is not enabled");

                return false;
            }

            if (logoutToken.IsNullOrWhiteSpace())
            {
                _logger.Debug("OIDC back-channel logout request is missing the logout token");

                return false;
            }

            var options = _oidcOptions.Get(nameof(AuthenticationType.Oidc));
            var configuration = options.Configuration ??
                                await options.ConfigurationManager.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

            var validationParameters = options.TokenValidationParameters.Clone();
            validationParameters.ValidIssuer = configuration.Issuer;
            validationParameters.ValidAudience = options.ClientId;
            validationParameters.IssuerSigningKeys = configuration.SigningKeys;
            validationParameters.RequireExpirationTime = false;

            var result = await new JsonWebTokenHandler().ValidateTokenAsync(logoutToken, validationParameters).ConfigureAwait(false);

            if (!result.IsValid)
            {
                _logger.Warn(result.Exception, "Invalid OIDC back-channel logout token");

                return false;
            }

            var token = (JsonWebToken)result.SecurityToken;

            if (!token.TryGetPayloadValue<long>("iat", out _))
            {
                _logger.Warn("OIDC back-channel logout token is missing the iat claim");

                return false;
            }

            if (!HasBackchannelLogoutEvent(token))
            {
                _logger.Warn("OIDC back-channel logout token is missing the back-channel logout event");

                return false;
            }

            if (token.TryGetPayloadValue<object>("nonce", out _))
            {
                _logger.Warn("OIDC back-channel logout token must not contain a nonce");

                return false;
            }

            token.TryGetPayloadValue<string>("sid", out var sessionId);
            token.TryGetPayloadValue<string>("sub", out var subject);

            if (sessionId.IsNotNullOrWhiteSpace())
            {
                _logger.Debug("Logging out OIDC session from back-channel logout request");
                _oidcLogoutService.LogoutSession(sessionId);

                return true;
            }

            if (subject.IsNotNullOrWhiteSpace())
            {
                _logger.Debug("Logging out all OIDC sessions for user from back-channel logout request");
                _oidcLogoutService.LogoutSubject(subject);

                return true;
            }

            _logger.Warn("OIDC back-channel logout token must contain a sid or sub claim");

            return false;
        }

        public bool IsLoggedOut(ClaimsPrincipal principal, DateTimeOffset? issuedUtc)
        {
            return _oidcLogoutService.IsLoggedOut(principal?.FindFirst("sid")?.Value,
                                                  principal?.FindFirst("sub")?.Value,
                                                  issuedUtc?.UtcDateTime);
        }

        private static bool HasBackchannelLogoutEvent(JsonWebToken token)
        {
            if (!token.TryGetPayloadValue<JsonElement>("events", out var events) ||
                events.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            return events.TryGetProperty(BackchannelLogoutEvent, out var logoutEvent) &&
                   logoutEvent.ValueKind == JsonValueKind.Object;
        }
    }
}
