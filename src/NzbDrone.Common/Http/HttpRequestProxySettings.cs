using System;
using System.Net;

namespace NzbDrone.Common.Http
{
    public class HttpRequestProxySettings
    {
        public HttpRequestProxySettings(ProxyType type, string host, int port, string filterSubnet, bool bypassLocalAddress, string username = null, string password = null)
        {
            Type = type;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            SubnetFilter = filterSubnet;
            BypassLocalAddress = bypassLocalAddress;
        }

        public ProxyType Type { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string SubnetFilter { get; private set; }
        public bool BypassLocalAddress { get; private set; }

        public string[] SubnetFilterAsArray
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SubnetFilter))
                {
                    return SubnetFilter.Split(';');
                }
                return new string[] { };
            }
        }

        public bool ShouldProxyBeBypassed(Uri url)
        {
            //We are utilising the WebProxy implementation here to save us having to reimplement it. This way we use Microsofts implementation
            WebProxy proxy = new WebProxy(Host + ":" + Port, BypassLocalAddress, SubnetFilterAsArray);

            return proxy.IsBypassed(url);
        }
    }
}
