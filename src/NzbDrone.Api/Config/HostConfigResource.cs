using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Update;
using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Api.Config
{
    public class HostConfigResource : RestResource
    {
        public string BindAddress { get; set; }
        public int Port { get; set; }
        public int SslPort { get; set; }
        public bool EnableSsl { get; set; }
        public bool LaunchBrowser { get; set; }
        public AuthenticationType AuthenticationMethod { get; set; }
        public bool AnalyticsEnabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LogLevel { get; set; }
        public string Branch { get; set; }
        public string ApiKey { get; set; }
        public bool Torrent { get; set; }
        public string SslCertHash { get; set; }
        public string UrlBase { get; set; }
        public bool UpdateAutomatically { get; set; }
        public UpdateMechanism UpdateMechanism { get; set; }
        public string UpdateScriptPath { get; set; }
        public bool ProxyEnabled { get; set; }
        public ProxyType ProxyType { get; set; }
        public string ProxyHostname { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public string ProxyBypassFilter { get; set; }
        public bool ProxyBypassLocalAddresses { get; set; }
    }
}
