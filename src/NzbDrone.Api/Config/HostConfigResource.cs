using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Update;

namespace NzbDrone.Api.Config
{
    public class HostConfigResource : RestResource
    {
        public String BindAddress { get; set; }
        public Int32 Port { get; set; }
        public Int32 SslPort { get; set; }
        public Boolean EnableSsl { get; set; }
        public Boolean LaunchBrowser { get; set; }
        public bool AuthenticationEnabled { get; set; }
        public Boolean AnalyticsEnabled { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public String LogLevel { get; set; }
        public String Branch { get; set; }
        public String ApiKey { get; set; }
        public Boolean Torrent { get; set; }
        public String SslCertHash { get; set; }
        public String UrlBase { get; set; }
        public Boolean UpdateAutomatically { get; set; }
        public UpdateMechanism UpdateMechanism { get; set; }
        public String UpdateScriptPath { get; set; }
    }
}
