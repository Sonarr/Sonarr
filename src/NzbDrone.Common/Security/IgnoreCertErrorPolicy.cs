using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NzbDrone.Common.Extensions;

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

            if (request != null &&  sslpolicyerrors != SslPolicyErrors.None &&
                (request.Address.OriginalString.ContainsIgnoreCase("nzbdrone.com") || request.Address.OriginalString.ContainsIgnoreCase("sonarr.tv"))
               )
            {
                return false;
            }

            return true;
        }
    }
}