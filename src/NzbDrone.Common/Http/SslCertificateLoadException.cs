using System.Security.Cryptography;

namespace NzbDrone.Common.Http;
public class SslCertificateLoadException : CryptographicException
{
    public SslCertificateLoadException(string message)
        : base(message)
    {
    }
}
