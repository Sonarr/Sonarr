using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Security
{
    public class X509CertificateValidationService : ICertificateValidationService
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public X509CertificateValidationService(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public bool ShouldByPassValidationError(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var targetHostName = string.Empty;

            if (sender is not SslStream && sender is not string)
            {
                return true;
            }

            if (sender is SslStream request)
            {
                targetHostName = request.TargetHostName;
            }

            // Mailkit passes host in sender as string
            if (sender is string stringHost)
            {
                targetHostName = stringHost;
            }

            if (certificate is X509Certificate2 cert2 && cert2.SignatureAlgorithm.FriendlyName == "md5RSA")
            {
                _logger.Error("https://{0} uses the obsolete md5 hash in it's https certificate, if that is your certificate, please (re)create certificate with better algorithm as soon as possible.", targetHostName);
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if (targetHostName == "localhost" || targetHostName == "127.0.0.1")
            {
                return true;
            }

            var ipAddresses = GetIPAddresses(targetHostName);
            var certificateValidation = _configService.CertificateValidation;

            if (certificateValidation == CertificateValidationType.Disabled)
            {
                return true;
            }

            if (certificateValidation == CertificateValidationType.DisabledForLocalAddresses &&
                ipAddresses.All(i => i.IsLocalAddress()))
            {
                return true;
            }

            _logger.Error("Certificate validation for {0} failed. {1}", targetHostName, sslPolicyErrors);

            return false;
        }

        private IPAddress[] GetIPAddresses(string host)
        {
            if (IPAddress.TryParse(host, out var ipAddress))
            {
                return new[] { ipAddress };
            }

            return Dns.GetHostEntry(host).AddressList;
        }
    }
}
