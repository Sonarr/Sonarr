using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http.Proxy
{
    public class HttpProxySettings
    {
        public HttpProxySettings(ProxyType type, string host, int port, string bypassFilter, bool bypassLocalAddress, string username = null, string password = null)
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

        public string[] BypassListAsArray
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BypassFilter))
                {
                    var hostlist = BypassFilter.Split(',');
                    for (int i = 0; i < hostlist.Length; i++)
                    {
                        if (hostlist[i].StartsWith("*"))
                        {
                            hostlist[i] = ";" + hostlist[i];
                        }
                    }

                    return hostlist;
                }

                return new string[] { };
            }
        }

        public string Key => string.Join("_",
            Type,
            Host,
            Port,
            Username,
            Password,
            BypassFilter,
            BypassLocalAddress);
    }
}
