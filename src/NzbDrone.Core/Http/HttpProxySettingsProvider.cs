using System;
using System.Net;
using NzbDrone.Common.Http;
using NzbDrone.Common.Http.Proxy;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Http
{
    public class HttpProxySettingsProvider : IHttpProxySettingsProvider
    {
        private readonly IConfigService _configService;

        public HttpProxySettingsProvider(IConfigService configService)
        {
            _configService = configService;
        }

        public HttpProxySettings GetProxySettings(HttpRequest request)
        {
            if (!_configService.ProxyEnabled)
            {
                return null;
            }
            
            var proxySettings = new HttpProxySettings(_configService.ProxyType,
                                _configService.ProxyHostname,
                                _configService.ProxyPort,
                                _configService.ProxyBypassFilter,
                                _configService.ProxyBypassLocalAddresses,
                                _configService.ProxyUsername,
                                _configService.ProxyPassword);

            if (ShouldProxyBeBypassed(proxySettings, request.Url))
            {
                return null;
            }

            return proxySettings;
        }

        public bool ShouldProxyBeBypassed(HttpProxySettings proxySettings, HttpUri url)
        {
            //We are utilising the WebProxy implementation here to save us having to reimplement it. This way we use Microsofts implementation
            var proxy = new WebProxy(proxySettings.Host + ":" + proxySettings.Port, proxySettings.BypassLocalAddress, proxySettings.BypassListAsArray);

            return proxy.IsBypassed((Uri)url);
        }
    }
}
