using System;
using System.Net;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public class HttpRequestProxySettings
    {
        public HttpRequestProxySettings(ProxyType type, string host, int port, string bypassFilter, bool bypassLocalAddress, string username = null, string password = null)
        {
            Type = type;
            Host = host.IsNullOrWhiteSpace() ? "127.0.0.1" : host;
            Port = port;
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
            BypassFilter = bypassFilter ?? string.Empty;
            BypassLocalAddress = bypassLocalAddress;
        }

        public ProxyType Type { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string BypassFilter { get; private set; }
        public bool BypassLocalAddress { get; private set; }

        public string[] SubnetFilterAsArray
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BypassFilter))
                {
                    return BypassFilter.Split(';');
                }
                return new string[] { };
            }
        }

        public string Key
        {
            get
            {
                return string.Join("_",
                    Type,
                    Host,
                    Port,
                    Username,
                    Password,
                    BypassFilter,
                    BypassLocalAddress);
            }
        }
    }
}
