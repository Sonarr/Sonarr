using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NzbDrone.Common.Http.Dispatchers
{
    public interface ICertificateValidationService
    {
        bool ShouldByPassValidationError(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);
    }
}
