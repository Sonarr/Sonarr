using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Http.Authentication
{
    public class UiAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private const string PolicyName = "UI";
        private readonly IConfigFileProvider _config;
        private readonly Logger _logger;

        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public UiAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options,
            IConfigFileProvider config,
            Logger logger)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _config = config;
            _logger = logger;
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.Equals(PolicyName, StringComparison.OrdinalIgnoreCase))
            {
                var authenticationMethod = _config.AuthenticationMethod;

#pragma warning disable CS0618 // Type or member is obsolete
                if (authenticationMethod == AuthenticationType.Basic)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    _logger.Error("Basic authentication method was removed, use Forms authentication instead.");

                    authenticationMethod = AuthenticationType.Forms;
                }

                var policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(authenticationMethod.ToString())
                    .AddRequirements(new BypassableDenyAnonymousAuthorizationRequirement());

                return Task.FromResult(policy.Build());
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
