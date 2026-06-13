namespace Sonarr.Http.Validation;

public interface ISslCertificateResource
{
    string SslCertPath { get; }
    string SslKeyPath { get; }
    string SslCertPassword { get; }
}
