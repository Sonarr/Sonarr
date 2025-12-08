using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
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
        public Results<RedirectHttpResult, ChallengeHttpResult> LoginSso([FromQuery] string returnUrl = null)
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
                new AuthenticationProperties { RedirectUri = GetRedirectUrl(returnUrl) },
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
                IsPersistent = resource.RememberMe == "on"
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
