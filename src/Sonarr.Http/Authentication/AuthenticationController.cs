using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public AuthenticationController(IAuthenticationService authService, IConfigFileProvider configFileProvider)
        {
            _authService = authService;
            _configFileProvider = configFileProvider;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginResource resource, [FromQuery] string returnUrl = null)
        {
            var user = _authService.Login(HttpContext.Request, resource.Username, resource.Password);

            if (user == null)
            {
                return Redirect($"~/login?returnUrl={returnUrl}&loginFailed=true");
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

            await HttpContext.SignInAsync(AuthenticationType.Forms.ToString(), new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "identifier")), authProperties);

            return Redirect(_configFileProvider.UrlBase + "/");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            _authService.Logout(HttpContext);
            await HttpContext.SignOutAsync(AuthenticationType.Forms.ToString());
            return Redirect(_configFileProvider.UrlBase + "/");
        }
    }
}
