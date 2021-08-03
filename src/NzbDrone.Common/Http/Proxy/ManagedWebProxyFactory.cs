using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
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
            switch (proxySettings.Type)
            {
                case ProxyType.Http:
                    if (proxySettings.Username.IsNotNullOrWhiteSpace() && proxySettings.Password.IsNotNullOrWhiteSpace())
                    {
                        return new WebProxy(proxySettings.Host + ":" + proxySettings.Port, proxySettings.BypassLocalAddress, proxySettings.BypassListAsArray, new NetworkCredential(proxySettings.Username, proxySettings.Password));
                    }
                    else
                    {
                        return new WebProxy(proxySettings.Host + ":" + proxySettings.Port, proxySettings.BypassLocalAddress, proxySettings.BypassListAsArray);
                    }

                case ProxyType.Socks4:
                    return new SocksWebProxy(new ProxyConfig(IPAddress.Loopback, GetNextFreePort(), GetProxyIpAddress(proxySettings.Host), proxySettings.Port, ProxyConfig.SocksVersion.Four, proxySettings.Username, proxySettings.Password), false);
                case ProxyType.Socks5:
                    return new SocksWebProxy(new ProxyConfig(IPAddress.Loopback, GetNextFreePort(), GetProxyIpAddress(proxySettings.Host), proxySettings.Port, ProxyConfig.SocksVersion.Five, proxySettings.Username, proxySettings.Password), false);
            }

            return null;
        }

        private static IPAddress GetProxyIpAddress(string host)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                try
                {
                    ipAddress = Dns.GetHostEntry(host).AddressList.OrderByDescending(a => a.AddressFamily == AddressFamily.InterNetwork).First();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format("Unable to resolve proxy hostname '{0}' to a valid IP address.", host), e);
                }
            }

            return ipAddress;
        }

        private static int GetNextFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}
