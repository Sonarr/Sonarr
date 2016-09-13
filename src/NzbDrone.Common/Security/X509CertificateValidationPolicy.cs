using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Security
{
    public static class X509CertificateValidationPolicy
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(X509CertificateValidationPolicy));

        public static void Register()
        {
            ServicePointManager.ServerCertificateValidationCallback = ShouldByPassValidationError;
        }

        private static bool ShouldByPassValidationError(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var request = sender as HttpWebRequest;

            if (request == null)
            {
                return true;
            }

            var req = sender as HttpWebRequest;
            var cert2 = certificate as X509Certificate2;
            if (cert2 != null && req != null && cert2.SignatureAlgorithm.FriendlyName == "md5RSA")
            {
                Logger.Error("https://{0} uses the obsolete md5 hash in it's https certificate, if that is your certificate, please (re)create certificate with better algorithm as soon as possible.", req.RequestUri.Authority);
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            Logger.Debug("Certificate validation for {0} failed. {1}", request.Address, sslPolicyErrors);

            return true;
        }
    }
}
