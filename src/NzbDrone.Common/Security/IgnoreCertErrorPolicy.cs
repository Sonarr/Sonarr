using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Security
{
    public static class IgnoreCertErrorPolicy
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger("IgnoreCertErrorPolicy");

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

            Logger.Error("Request for {0} failed certificated validation. {1}", request.Address, sslpolicyerrors);

            return true;
        }
    }
}