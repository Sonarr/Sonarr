using System.Security.Cryptography;

namespace NzbDrone.Common.Http;

public class SslCertificateLoadException : CryptographicException
{
    public SslCertificateLoadException()
        : base("SSL Certificate Load Exception")
    {
    }

    public SslCertificateLoadException(string message)
        : base(message)
    {
    }

    public SslCertificateLoadException(string message, System.Exception innerException)
        : base(message, innerException)
    {
    }
}
