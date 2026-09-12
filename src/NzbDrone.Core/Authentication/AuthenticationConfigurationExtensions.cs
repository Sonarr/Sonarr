using System;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Authentication
{
    public static class AuthenticationConfigurationExtensions
    {
        public static bool IsOidcConfigured(this IConfigFileProvider configFileProvider)
        {
            return configFileProvider.HasOidcSettings() && configFileProvider.IsOidcAuthoritySecure();
        }

        public static bool HasOidcSettings(this IConfigFileProvider configFileProvider)
        {
            return configFileProvider.OidcAuthority.IsNotNullOrWhiteSpace() &&
                   configFileProvider.OidcClientId.IsNotNullOrWhiteSpace() &&
                   configFileProvider.OidcClientSecret.IsNotNullOrWhiteSpace() &&
                   configFileProvider.OidcUserIdentifier.IsNotNullOrWhiteSpace() &&
                   GetOidcScopes(configFileProvider.OidcScopes).Contains("openid");
        }

        public static bool IsOidcAuthoritySecure(this IConfigFileProvider configFileProvider)
        {
            return configFileProvider.OidcAuthority.IsNotNullOrWhiteSpace() &&
                   configFileProvider.OidcAuthority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public static string[] GetOidcScopes(string scopes)
        {
            return scopes?.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        }

        public static AuthenticationType EffectiveAuthenticationMethod(this IConfigFileProvider configFileProvider)
        {
            var authenticationMethod = configFileProvider.AuthenticationMethod;

            return authenticationMethod == AuthenticationType.Oidc && !configFileProvider.IsOidcConfigured()
                ? AuthenticationType.Forms
                : authenticationMethod;
        }
    }
}
