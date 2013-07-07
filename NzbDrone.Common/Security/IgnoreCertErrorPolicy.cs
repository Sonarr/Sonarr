using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Security
{
    public static class IgnoreCertErrorPolicy
    {
        private static readonly Logger Logger = LogManager.GetLogger("CertPolicy");

        public static void Register()
        {
            if (OsInfo.IsLinux)
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidationCallback;
            }
        }

        private static bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            Logger.Warn("[{0}] {1}", sender.GetType(), sslpolicyerrors);
            return true;
        }
    }
}