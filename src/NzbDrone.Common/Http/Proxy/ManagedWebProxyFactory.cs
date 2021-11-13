using System;
using System.Net;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http.Proxy
{
    public interface ICreateManagedWebProxy
    {
        IWebProxy GetWebProxy(HttpProxySettings proxySettings);
    }

    public class ManagedWebProxyFactory : ICreateManagedWebProxy
    {
        private readonly ICached<IWebProxy> _webProxyCache;

        public ManagedWebProxyFactory(ICacheManager cacheManager)
        {
            _webProxyCache = cacheManager.GetCache<IWebProxy>(GetType(), "webProxy");
        }

        public IWebProxy GetWebProxy(HttpProxySettings proxySettings)
        {
            var proxy = _webProxyCache.Get(proxySettings.Key, () => CreateWebProxy(proxySettings), TimeSpan.FromMinutes(5));

            _webProxyCache.ClearExpired();

            return proxy;
        }

        private IWebProxy CreateWebProxy(HttpProxySettings proxySettings)
        {
            var uri = GetProxyUri(proxySettings);

            if (uri == null)
            {
                return null;
            }

            if (proxySettings.Username.IsNotNullOrWhiteSpace() && proxySettings.Password.IsNotNullOrWhiteSpace())
            {
                return new WebProxy(uri, proxySettings.BypassLocalAddress, proxySettings.BypassListAsArray, new NetworkCredential(proxySettings.Username, proxySettings.Password));
            }
            else
            {
                return new WebProxy(uri, proxySettings.BypassLocalAddress, proxySettings.BypassListAsArray);
            }
        }

        private Uri GetProxyUri(HttpProxySettings proxySettings)
        {
            switch (proxySettings.Type)
            {
                case ProxyType.Http:
                    return new Uri("http://" + proxySettings.Host + ":" + proxySettings.Port);
                case ProxyType.Socks4:
                    return new Uri("socks4://" + proxySettings.Host + ":" + proxySettings.Port);
                case ProxyType.Socks5:
                    return new Uri("socks5://" + proxySettings.Host + ":" + proxySettings.Port);
                default:
                    return null;
            }
        }
    }
}
