using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Authentication
{
    public record OidcExchangeRequest([property: JsonPropertyName("subject_token")] string SubjectToken, [property: JsonPropertyName("subjectToken")] string SubjectTokenCamel = null)
    {
        public string ResolvedSubjectToken => string.IsNullOrWhiteSpace(SubjectToken) ? SubjectTokenCamel : SubjectToken;
    }

    [AllowAnonymous]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly Logger _logger;

        public AuthenticationController(IAuthenticationService authService, IConfigFileProvider configFileProvider, IAppFolderInfo appFolderInfo, Logger logger)
        {
            _authService = authService;
            _configFileProvider = configFileProvider;
            _appFolderInfo = appFolderInfo;
            _logger = logger;
        }

        [HttpGet("login/sso")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public Results<RedirectHttpResult, ChallengeHttpResult> LoginSso([FromQuery] string returnUrl = null, [FromQuery] bool rememberMe = false)
        {
            if (_configFileProvider.AuthenticationMethod != AuthenticationType.Oidc)
            {
                return TypedResults.Redirect(_configFileProvider.UrlBase + "/login");
            }

            if (!_configFileProvider.IsOidcConfigured())
            {
                _logger.Error("OIDC authentication is enabled, but Authority, Client ID, Client Secret, User and Scopes are not all configured");

                return TypedResults.Redirect(_configFileProvider.UrlBase + "/login");
            }

            return TypedResults.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = GetRedirectUrl(returnUrl),
                    IsPersistent = rememberMe
                },
                [nameof(AuthenticationType.Oidc)]);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/json")]
        public async Task<Results<RedirectHttpResult, UnauthorizedHttpResult>> Login([FromForm] LoginResource resource, [FromQuery] string returnUrl = null)
        {
            var user = _authService.Login(HttpContext.Request, resource.Username, resource.Password);

            if (user == null)
            {
                var lockoutEndTime = _authService.GetLockoutEndTime(HttpContext.Request);

                if (lockoutEndTime.HasValue)
                {
                    var lockoutUntil = new DateTimeOffset(lockoutEndTime.Value, TimeSpan.Zero).ToUnixTimeMilliseconds();

                    return TypedResults.Redirect($"~/login?returnUrl={returnUrl}&loginFailed=true&lockoutUntil={lockoutUntil}");
                }

                return TypedResults.Redirect($"~/login?returnUrl={returnUrl}&loginFailed=true");
            }

            var claims = new List<Claim>
            {
                new Claim("user", user.Username),
                new Claim("identifier", user.Identifier.ToString()),
                new Claim("AuthType", AuthenticationType.Forms.ToString())
            };

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = resource.RememberMe
            };

            try
            {
                await HttpContext.SignInAsync(AuthenticationType.Forms.ToString(), new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "identifier")), authProperties);
            }
            catch (CryptographicException e)
            {
                if (e.InnerException is XmlException)
                {
                    _logger.Error(e, "Failed to authenticate user due to corrupt XML. Please remove all XML files from {0} and restart Sonarr", Path.Combine(_appFolderInfo.AppDataFolder, "asp"));
                }
                else
                {
                    _logger.Error(e, "Failed to authenticate user. {0}", e.Message);
                }

                return TypedResults.Unauthorized();
            }

            return TypedResults.Redirect(GetRedirectUrl(returnUrl));
        }

        [HttpPost("reset-password/request")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public RedirectHttpResult RequestPasswordReset()
        {
            var token = _authService.RequestPasswordReset(HttpContext.Request);

            if (token == null)
            {
                return TypedResults.Redirect("~/login?resetPassword=true&tokenSent=true&tokenUnavailable=true");
            }

            return TypedResults.Redirect("~/login?resetPassword=true&tokenSent=true");
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public RedirectHttpResult ResetPassword([FromForm] ResetPasswordResource resource)
        {
            if (resource.Username.IsNullOrWhiteSpace() ||
                resource.Password.IsNullOrWhiteSpace() ||
                resource.Password != resource.PasswordConfirmation)
            {
                return TypedResults.Redirect("~/login?resetPassword=true&tokenSent=true&resetInvalid=true");
            }

            if (!_authService.ResetPassword(HttpContext.Request, resource.Token, resource.Username, resource.Password))
            {
                return TypedResults.Redirect("~/login?resetPassword=true&tokenSent=true&resetFailed=true");
            }

            return TypedResults.Redirect("~/login?passwordReset=true");
        }

        [HttpGet("logout")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<Results<RedirectHttpResult, EmptyHttpResult>> Logout()
        {
            _authService.Logout(HttpContext);

            if (_configFileProvider.EffectiveAuthenticationMethod() == AuthenticationType.Oidc)
            {
                var loggedOutUrl = _configFileProvider.UrlBase + "/loggedout";
                var signedOut = false;

                try
                {
                    await HttpContext.SignOutAsync(nameof(AuthenticationType.Oidc), new AuthenticationProperties { RedirectUri = loggedOutUrl });

                    signedOut = true;
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Unable to sign out of the OIDC provider, signing out locally only");
                }

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(AuthenticationType.Forms.ToString());

                if (signedOut || Response.HasStarted)
                {
                    return TypedResults.Empty;
                }

                return TypedResults.Redirect(loggedOutUrl);
            }

            await HttpContext.SignOutAsync(AuthenticationType.Forms.ToString());
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return TypedResults.Redirect(_configFileProvider.UrlBase + "/");
        }

        private static readonly ConcurrentDictionary<string, Queue<DateTime>> ExchangeRateLimits = new(StringComparer.Ordinal);
        private static readonly object ExchangeRateLock = new();

        private static bool TryAcquireExchangeRateLimit(string clientIp)
        {
            var now = DateTime.UtcNow;
            var window = TimeSpan.FromMinutes(1);

            lock (ExchangeRateLock)
            {
                if (!ExchangeRateLimits.TryGetValue(clientIp, out var queue))
                {
                    queue = new Queue<DateTime>();
                    ExchangeRateLimits[clientIp] = queue;
                }

                while (queue.Count > 0 && (now - queue.Peek()) > window)
                {
                    queue.Dequeue();
                }

                if (queue.Count >= 10)
                {
                    return false;
                }

                queue.Enqueue(now);
                return true;
            }
        }

        [HttpPost("api/auth/oidc/exchange")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IResult> ExchangeOidc([FromBody] OidcExchangeRequest request)
        {
            var httpContext = HttpContext ?? new DefaultHttpContext();

            var clientIp = httpContext.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(clientIp))
            {
                clientIp = "unknown";
            }

            if (!TryAcquireExchangeRateLimit(clientIp))
            {
                return TypedResults.Json(new { message = "rate_limit_exceeded" }, statusCode: StatusCodes.Status429TooManyRequests);
            }

            var subjectToken = request?.ResolvedSubjectToken;

            if (string.IsNullOrWhiteSpace(subjectToken))
            {
                var authHeader = httpContext.Request.Headers.Authorization.ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    subjectToken = authHeader.Substring(7).Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(subjectToken))
            {
                return TypedResults.Json(new { message = "subject_token is required" }, statusCode: StatusCodes.Status400BadRequest);
            }

            var oidc = httpContext.RequestServices?.GetService(typeof(IOidcTokenValidationService)) as IOidcTokenValidationService;
            if (oidc == null)
            {
                return TypedResults.Json(new { message = "service_unavailable" }, statusCode: StatusCodes.Status500InternalServerError);
            }

            var validation = await oidc.ValidateAsync(subjectToken);
            if (!validation.Success)
            {
                return TypedResults.Json(new { message = "invalid_token" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var role = OidcGroupMapping.Resolve(validation.Groups, _configFileProvider.OidcRoleMappings);
            if (role == null)
            {
                return TypedResults.Json(new { message = "insufficient_group" }, statusCode: StatusCodes.Status403Forbidden);
            }

            var userService = httpContext.RequestServices?.GetService(typeof(IUserService)) as IUserService;
            try
            {
                userService?.UpsertOidcUser(validation.Subject, validation.Username, string.Join(",", validation.Groups), role.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to upsert OIDC user");
                return TypedResults.Json(new { message = "User provisioning failed" }, statusCode: StatusCodes.Status500InternalServerError);
            }

            var tokenService = httpContext.RequestServices?.GetService(typeof(ISonarrServiceTokenService)) as ISonarrServiceTokenService;
            var sonarrToken = tokenService != null ? tokenService.MintToken(validation.Subject, role.Value) : null;

            return TypedResults.Json(new
            {
                token = sonarrToken,
                token_type = "Bearer",
                expires_in = 300,
                role = role.Value.ToString(),
                permissions = SonarrRolePermissions.ForRole(role.Value),
            },
            statusCode: StatusCodes.Status200OK);
        }

        [HttpGet("api/auth/me")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IResult GetMe()
        {
            var httpContext = HttpContext ?? new DefaultHttpContext();

            var authHeader = httpContext.Request.Headers.Authorization.ToString();
            string token = null;
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring(7).Trim();
            }

            var tokenService = httpContext.RequestServices?.GetService(typeof(ISonarrServiceTokenService)) as ISonarrServiceTokenService;
            var principal = tokenService != null ? tokenService.ValidateToken(token) : null;
            if (principal == null)
            {
                return TypedResults.Json(new { message = "invalid_token" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var sub = principal.FindFirst("sub")?.Value;

            var repo = httpContext.RequestServices?.GetService(typeof(IUserRepository)) as IUserRepository;
            var user = repo != null && sub != null ? repo.FindBySubject(sub) : null;
            if (user == null)
            {
                return TypedResults.Json(new { message = "not_found" }, statusCode: StatusCodes.Status404NotFound);
            }

            var groups = string.IsNullOrWhiteSpace(user.OidcGroups)
                ? new List<string>()
                : user.OidcGroups.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            return TypedResults.Json(new
            {
                username = user.Username,
                role = user.Role.ToString(),
                groups,
                permissions = SonarrRolePermissions.ForRole(user.Role),
            },
            statusCode: StatusCodes.Status200OK);
        }

        private string GetRedirectUrl(string returnUrl)
        {
            var urlBase = _configFileProvider.UrlBase;

            if (returnUrl.IsNullOrWhiteSpace() || !Url.IsLocalUrl(returnUrl))
            {
                return urlBase + "/";
            }

            if (urlBase.IsNullOrWhiteSpace() ||
                returnUrl.Equals(urlBase, StringComparison.OrdinalIgnoreCase) ||
                returnUrl.StartsWith(urlBase + "/", StringComparison.OrdinalIgnoreCase))
            {
                return returnUrl;
            }

            return urlBase + returnUrl;
        }
    }
}
