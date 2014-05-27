using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NzbDrone.Common.Security
{
    public static class IgnoreCertErrorPolicy
    {
        public static void Register()
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidationCallback;
        }

        private static bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            var request = sender as HttpWebRequest;

            if (request != null &&
                request.Address.OriginalString.ContainsIgnoreCase("nzbdrone.com") &&
                sslpolicyerrors != SslPolicyErrors.None)
            {
                return false;
            }

            return true;
        }
    }
}