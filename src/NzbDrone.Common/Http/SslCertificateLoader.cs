using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public class SslCertificateLoadException : CryptographicException
    {
        public SslCertificateLoadException(string message)
            : base(message)
        {
        }
    }

    public static class SslCertificateLoader
    {
        public static SslStreamCertificateContext LoadCertificateContext(string certPath, string keyPath, string certPassword)
        {
            X509Certificate2Collection certificateCollection;
            X509Certificate2 leafCert;

            var type = X509Certificate2.GetCertContentType(certPath);
            if (type == X509ContentType.Cert)
            {
                leafCert = X509Certificate2.CreateFromPemFile(certPath, keyPath.IsNullOrWhiteSpace() ? null : keyPath);

                certificateCollection = new X509Certificate2Collection();
                certificateCollection.ImportFromPemFile(certPath);

                var duplicate = certificateCollection.FirstOrDefault(c => c.SerialNumber == leafCert.SerialNumber);

                if (duplicate != null)
                {
                    certificateCollection.Remove(duplicate);
                }

                certificateCollection.Insert(0, leafCert);
            }
            else if (type == X509ContentType.Pkcs12)
            {
                certificateCollection = X509CertificateLoader.LoadPkcs12CollectionFromFile(certPath, certPassword, X509KeyStorageFlags.DefaultKeySet);
                leafCert = certificateCollection.FirstOrDefault(c => c.HasPrivateKey);
            }
            else
            {
                throw new SslCertificateLoadException($"Invalid certificate type: {type}");
            }

            if (leafCert == null)
            {
                throw new SslCertificateLoadException(
                    $"The SSL certificate file {certPath} does not contain a certificate with an associated private key");
            }

            return SslStreamCertificateContext.Create(leafCert, certificateCollection, offline: true);
        }
    }
}
