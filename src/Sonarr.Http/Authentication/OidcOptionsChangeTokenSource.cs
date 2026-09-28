using System.Threading;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Messaging.Events;

namespace Sonarr.Http.Authentication
{
    public class OidcOptionsChangeTokenSource : IOptionsChangeTokenSource<OpenIdConnectOptions>, IHandle<ConfigFileSavedEvent>
    {
        private readonly IConfigFileProvider _configFileProvider;

        private ConfigurationReloadToken _changeToken = new ConfigurationReloadToken();
        private string _settings;

        public OidcOptionsChangeTokenSource(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
            _settings = GetSettings();
        }

        public string Name => nameof(AuthenticationType.Oidc);

        public IChangeToken GetChangeToken()
        {
            return _changeToken;
        }

        public void Handle(ConfigFileSavedEvent message)
        {
            var settings = GetSettings();

            if (settings == _settings)
            {
                return;
            }

            _settings = settings;

            Interlocked.Exchange(ref _changeToken, new ConfigurationReloadToken()).OnReload();
        }

        private string GetSettings()
        {
            return string.Join('|',
                _configFileProvider.AuthenticationMethod,
                _configFileProvider.OidcAuthority,
                _configFileProvider.OidcClientId,
                _configFileProvider.OidcClientSecret,
                _configFileProvider.OidcUserIdentifier,
                _configFileProvider.OidcScopes,
                _configFileProvider.UrlBase);
        }
    }
}
