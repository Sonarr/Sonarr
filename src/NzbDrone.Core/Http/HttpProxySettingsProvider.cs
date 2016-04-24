using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Http
{
    public class HttpProxySettingsProvider : IHttpProxySettingsProvider
    {
        private readonly IConfigService _configService;

        public HttpProxySettingsProvider(IConfigService configService)
        {
            _configService = configService;
        }

        public HttpRequestProxySettings GetProxySettings(HttpRequest request)
        {
            if (!_configService.ProxyEnabled)
            {
                return null;
            }
            
            var proxySettings = new HttpRequestProxySettings(_configService.ProxyType,
                                _configService.ProxyHostname,
                                _configService.ProxyPort,
                                _configService.ProxySubnetFilter,
                                _configService.ProxyBypassLocalAddresses,
                                _configService.ProxyUsername,
                                _configService.ProxyPassword);

            if (ShouldProxyBeBypassed(proxySettings, request.Url))
            {
                return null;
            }

            return proxySettings;
        }

        public bool ShouldProxyBeBypassed(HttpRequestProxySettings proxySettings, HttpUri url)
        {
            //We are utilising the WebProxy implementation here to save us having to reimplement it. This way we use Microsofts implementation
            var proxy = new WebProxy(proxySettings.Host + ":" + proxySettings.Port, proxySettings.BypassLocalAddress, proxySettings.SubnetFilterAsArray);

            return proxy.IsBypassed((Uri)url);
        }
    }
}
