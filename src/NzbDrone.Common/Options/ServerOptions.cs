namespace NzbDrone.Common.Options;

public class ServerOptions
{
    public string UrlBase { get; set; }
    public string BindAddress { get; set; }
    public int? Port { get; set; }
    public bool? EnableSsl { get; set; }
    public int? SslPort { get; set; }
    public string SslCertPath { get; set; }
    public string SslCertPassword { get; set; }
}
