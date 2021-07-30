using NzbDrone.Common.Http.Proxy;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Security;
using NzbDrone.Core.Update;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Config
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
        public string ConsoleLogLevel { get; set; }
        public string Branch { get; set; }
        public string ApiKey { get; set; }
        public string SslCertPath { get; set; }
        public string SslCertPassword { get; set; }
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
        public CertificateValidationType CertificateValidation { get; set; }
        public string BackupFolder { get; set; }
        public int BackupInterval { get; set; }
        public int BackupRetention { get; set; }
    }

    public static class HostConfigResourceMapper
    {
        public static HostConfigResource ToResource(this IConfigFileProvider model, IConfigService configService)
        {
            // TODO: Clean this mess up. don't mix data from multiple classes, use sub-resources instead?
            return new HostConfigResource
            {
                BindAddress = model.BindAddress,
                Port = model.Port,
                SslPort = model.SslPort,
                EnableSsl = model.EnableSsl,
                LaunchBrowser = model.LaunchBrowser,
                AuthenticationMethod = model.AuthenticationMethod,
                AnalyticsEnabled = model.AnalyticsEnabled,
                //Username
                //Password
                LogLevel = model.LogLevel,
                ConsoleLogLevel = model.ConsoleLogLevel,
                Branch = model.Branch,
                ApiKey = model.ApiKey,
                SslCertPath = model.SslCertPath,
                SslCertPassword = model.SslCertPassword,
                UrlBase = model.UrlBase,
                UpdateAutomatically = model.UpdateAutomatically,
                UpdateMechanism = model.UpdateMechanism,
                UpdateScriptPath = model.UpdateScriptPath,
                ProxyEnabled = configService.ProxyEnabled,
                ProxyType = configService.ProxyType,
                ProxyHostname = configService.ProxyHostname,
                ProxyPort = configService.ProxyPort,
                ProxyUsername = configService.ProxyUsername,
                ProxyPassword = configService.ProxyPassword,
                ProxyBypassFilter = configService.ProxyBypassFilter,
                ProxyBypassLocalAddresses = configService.ProxyBypassLocalAddresses,
                CertificateValidation = configService.CertificateValidation,
                BackupFolder = configService.BackupFolder,
                BackupInterval = configService.BackupInterval,
                BackupRetention = configService.BackupRetention
            };
        }
    }
}
