using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Security
{
    public interface IX509CertificateValidationPolicy
    {
        void Register();
    }

    public class X509CertificateValidationPolicy : IX509CertificateValidationPolicy
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public X509CertificateValidationPolicy(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public void Register()
        {
            ServicePointManager.ServerCertificateValidationCallback = ShouldByPassValidationError;
        }

        private bool ShouldByPassValidationError(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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
                _logger.Error("https://{0} uses the obsolete md5 hash in it's https certificate, if that is your certificate, please (re)create certificate with better algorithm as soon as possible.", req.RequestUri.Authority);
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            var host = Dns.GetHostEntry(req.Host);
            var certificateValidation = _configService.CertificateValidation;

            if (certificateValidation == CertificateValidationType.Disabled)
            {
                return true;
            }

            if (certificateValidation == CertificateValidationType.DisabledForLocalAddresses && host.AddressList.All(i => i.IsLocalAddress()))
            {
                return true;
            }


            _logger.Error("Certificate validation for {0} failed. {1}", request.Address, sslPolicyErrors);

            return false;
        }
    }
}
