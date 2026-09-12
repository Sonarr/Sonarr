using System.Collections.Generic;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ApplicationStartedEvent))]
    [CheckOn(typeof(ConfigFileSavedEvent))]
    public class OidcConfigurationCheck : HealthCheckBase
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IOidcDiscoveryService _oidcDiscoveryService;

        public OidcConfigurationCheck(IConfigFileProvider configFileProvider,
            IOidcDiscoveryService oidcDiscoveryService,
            ILocalizationService localizationService)
            : base(localizationService)
        {
            _configFileProvider = configFileProvider;
            _oidcDiscoveryService = oidcDiscoveryService;
        }

        public override HealthCheck Check()
        {
            if (_configFileProvider.AuthenticationMethod != AuthenticationType.Oidc)
            {
                return new HealthCheck(GetType());
            }

            if (!_configFileProvider.HasOidcSettings())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.OidcNotConfigured,
                    _localizationService.GetLocalizedString("OidcNotConfiguredHealthCheckMessage"),
                    "#oidc-not-configured");
            }

            if (!_configFileProvider.IsOidcAuthoritySecure())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.OidcAuthorityNotSecure,
                    _localizationService.GetLocalizedString("OidcAuthorityNotSecureHealthCheckMessage"),
                    "#oidc-authority-not-secure");
            }

            if (!_oidcDiscoveryService.IsDiscoverable(_configFileProvider.OidcAuthority))
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.OidcDiscoveryFailed,
                    _localizationService.GetLocalizedString("OidcDiscoveryFailedHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "authority", _configFileProvider.OidcAuthority }
                    }),
                    "#oidc-configuration-unavailable");
            }

            return new HealthCheck(GetType());
        }
    }
}
