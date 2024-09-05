using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";

        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;

        public string HeaderName { get; set; }
        public string QueryName { get; set; }
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly UserService _userService;

        private readonly IConfigFileProvider _configFileProvider;

        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            UserService userService,
            IConfigFileProvider configFileProvider)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
            _configFileProvider = configFileProvider;
        }

        private string ParseApiKey()
        {
            // Try query parameter
            if (Request.Query.TryGetValue(Options.QueryName, out var value))
            {
                return value.FirstOrDefault();
            }

            // No ApiKey query parameter found try headers
            if (Request.Headers.TryGetValue(Options.HeaderName, out var headerValue))
            {
                return headerValue.FirstOrDefault();
            }

            return Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var providedApiKey = ParseApiKey();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // If the authentication method is set to None, this means the app has not been setup yet and allow all requests until setup is complete.
            var user = _userService.FindUserFromApiKey(providedApiKey);
            var authMethod = _configFileProvider.AuthenticationMethod;
            if (user != null || authMethod == AuthenticationType.None)
            {
                var claims = new List<Claim>
                {
                    new Claim("ApiKey", "true"),
                    new Claim(ClaimTypes.Role, user != null ? user.Role.ToString() : "Admin")
                };

                var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
                var identities = new List<ClaimsIdentity> { identity };
                var principal = new ClaimsPrincipal(identities);
                var ticket = new AuthenticationTicket(principal, Options.Scheme);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            return Task.CompletedTask;
        }
    }
}
