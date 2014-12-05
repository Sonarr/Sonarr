using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Security
{
    public static class IgnoreCertErrorPolicy
    {
        private static Logger _logger = NzbDroneLogger.GetLogger("IgnoreCertErrorPolicy");

        public static void Register()
        {
            ServicePointManager.ServerCertificateValidationCallback = ShouldByPassValidationError;
        }

        private static bool ShouldByPassValidationError(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            var request = sender as HttpWebRequest;

            if (request == null)
            {
                return true;
            }

            if (sslpolicyerrors == SslPolicyErrors.None)
            {
                return true;
            }

            _logger.Warn("Request for {0} failed certificated validation. {1}", request.Address, sslpolicyerrors);

            if (OsInfo.IsMono)
            {
                return true;
            }

            var host = request.Address.Host.ToLower();

            if (host.EndsWith("nzbdrone.com") || host.EndsWith("sonarr.tv"))
            {
                return false;
            }

            return true;
        }
    }
}