using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using NLog;

namespace Sonarr.Http.Authentication
{
    public record OidcValidationResult(bool Success, string FailureReason, string Subject, string Username, List<string> Groups);

    public interface IOidcTokenValidationService
    {
        Task<OidcValidationResult> ValidateAsync(string token);
    }

    public class OidcTokenValidationService : IOidcTokenValidationService
    {
        private static readonly Logger Logger = LogManager.GetLogger(nameof(OidcTokenValidationService));

        // Raw claim names are read below ("sub", "preferred_username", "groups"), so inbound
        // claim mapping must stay off to match the JWT-bearer path in
        // AuthenticationBuilderExtensions.cs. Mapping would rename "sub" to
        // ClaimTypes.NameIdentifier and every valid token would fail as missing_subject.
        private readonly JwtSecurityTokenHandler _handler = new() { MapInboundClaims = false };
        private readonly NzbDrone.Core.Configuration.IConfigFileProvider _configFileProvider;

        // Set from DI (network-backed) or supplied directly as the test seam. Not registered
        // in DI itself, hence the optional parameter.
        private IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        // Single construction path: both dependencies are assigned when supplied. Production
        // DI passes only the options monitor and builds the manager lazily; tests pass both the
        // manager and the monitor so OidcClientId is available and discovery never hits the network.
        public OidcTokenValidationService(
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = null,
            IOptionsMonitor<NzbDrone.Core.Configuration.IConfigFileProvider> configProvider = null)
        {
            _configurationManager = configurationManager;
            _configFileProvider = configProvider?.CurrentValue;
        }

        public async Task<OidcValidationResult> ValidateAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return InvalidToken();
            }

            var cleanedToken = token.Trim();
            if (cleanedToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                cleanedToken = cleanedToken.Substring(7).Trim();
            }

            // ValidateAsync must never throw: AuthenticationController.ExchangeOidc has no
            // catch, so any escaped exception would surface as 500 instead of the documented 401.
            try
            {
                return await ValidateCoreAsync(cleanedToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "OIDC token validation failed; rejecting token");
                return InvalidToken();
            }
        }

        private async Task<OidcValidationResult> ValidateCoreAsync(string cleanedToken)
        {
            var configuration = await GetConfigurationAsync().ConfigureAwait(false);

            if (configuration == null)
            {
                Logger.Debug("OIDC token validation skipped: discovery returned null");
                return InvalidToken();
            }

            var expectedAudience = _configFileProvider?.OidcClientId?.Trim();

            // Audience rule: clientId set → exact match only; unset/empty → fail closed (no validation attempted)
            if (string.IsNullOrWhiteSpace(expectedAudience))
            {
                Logger.Debug("OIDC token validation skipped: no client ID configured");
                return InvalidToken();
            }

            var signingKeys = GetSigningKeys(configuration);
            if (!signingKeys.Any())
            {
                Logger.Debug("OIDC token validation skipped: no signing keys available");
                return InvalidToken();
            }

            var validationResult = await ValidateTokenAsync(cleanedToken, configuration, signingKeys, expectedAudience).ConfigureAwait(false);

            // An unknown kid or a stale key is the expected result of key rotation. Ask the
            // configuration manager to refresh and retry once inside this call so a rotated
            // key does not fail the first exchange after rotation.
            if (!validationResult.IsValid && IsSignatureFailure(validationResult.Exception))
            {
                Logger.Debug(validationResult.Exception, "OIDC signing key not found or signature invalid; refreshing JWKS and retrying once");

                _configurationManager.RequestRefresh();
                configuration = await _configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);

                if (configuration == null)
                {
                    Logger.Debug("OIDC token validation skipped: refresh returned null");
                    return InvalidToken();
                }

                signingKeys = GetSigningKeys(configuration);
                if (!signingKeys.Any())
                {
                    Logger.Debug("OIDC token validation skipped: no signing keys available after refresh");
                    return InvalidToken();
                }

                validationResult = await ValidateTokenAsync(cleanedToken, configuration, signingKeys, expectedAudience).ConfigureAwait(false);
            }

            if (!validationResult.IsValid)
            {
                Logger.Debug(validationResult.Exception, "OIDC token rejected");
                return InvalidToken();
            }

            var identity = validationResult.ClaimsIdentity;
            var subject = identity?.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(subject))
            {
                return new OidcValidationResult(false, "missing_subject", null, null, new List<string>());
            }

            var username = identity.FindFirst("preferred_username")?.Value
                ?? identity.FindFirst("email")?.Value
                ?? subject;

            var groups = AuthenticationBuilderExtensions.GetGroups(new ClaimsPrincipal(identity), "groups").ToList();

            return new OidcValidationResult(true, null, subject, username, groups);
        }

        private async Task<OpenIdConnectConfiguration> GetConfigurationAsync()
        {
            // Lazy-init ConfigurationManager on first call; fail closed if OIDC not configured
            if (_configurationManager == null)
            {
                var authority = _configFileProvider?.OidcAuthority;

                // Fail closed: no configuration, return invalid_token immediately
                if (string.IsNullOrWhiteSpace(authority))
                {
                    Logger.Debug("OIDC token validation skipped: Authority not configured");
                    return null;
                }

                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    authority.TrimEnd('/') + "/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever())
                {
                    AutomaticRefreshInterval = TimeSpan.FromHours(24),
                    RefreshInterval = TimeSpan.FromHours(24),
                };
            }

            return await _configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<TokenValidationResult> ValidateTokenAsync(
            string token,
            OpenIdConnectConfiguration configuration,
            List<SecurityKey> signingKeys,
            string expectedAudience)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration.Issuer,
                ValidateAudience = true,
                ValidAudience = expectedAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                AudienceValidator = (audiences, securityToken, validationParameters) =>
                    audiences != null && audiences.Any(a => string.Equals(a, expectedAudience, StringComparison.Ordinal)),
            };

            return await _handler.ValidateTokenAsync(token, parameters).ConfigureAwait(false);
        }

        private static List<SecurityKey> GetSigningKeys(OpenIdConnectConfiguration configuration)
        {
            var signingKeys = new List<SecurityKey>();

            if (configuration.SigningKeys != null)
            {
                signingKeys.AddRange(configuration.SigningKeys);
            }

            if (configuration.JsonWebKeySet != null)
            {
                try
                {
                    signingKeys.AddRange(configuration.JsonWebKeySet.GetSigningKeys());
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Failed to extract signing keys from JWKS");
                }
            }

            return signingKeys;
        }

        private static bool IsSignatureFailure(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }

            if (exception is AggregateException aggregate)
            {
                return aggregate.Flatten().InnerExceptions.Any(IsSignatureFailure);
            }

            return exception is SecurityTokenSignatureKeyNotFoundException
                || exception is SecurityTokenInvalidSignatureException
                || IsSignatureFailure(exception.InnerException);
        }

        private static OidcValidationResult InvalidToken()
        {
            return new OidcValidationResult(false, "invalid_token", null, null, new List<string>());
        }
    }
}
