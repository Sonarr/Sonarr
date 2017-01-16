using System;
using System.Net;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Security
{
    public static class SecurityProtocolPolicy
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(SecurityProtocolPolicy));

        private const SecurityProtocolType Tls11 = (SecurityProtocolType)768;
        private const SecurityProtocolType Tls12 = (SecurityProtocolType)3072;

        public static void Register()
        {
            try
            {
                // TODO: In v3 we should drop support for SSL3 because its very insecure. Only leaving it enabled because some people might rely on it.
                var protocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

                if (Enum.IsDefined(typeof(SecurityProtocolType), Tls11))
                {
                    protocol |= Tls11;
                }

                // Enabling Tls1.2 invalidates certificates using md5, so we disable Tls12 on the fly if that happens.
                if (Enum.IsDefined(typeof(SecurityProtocolType), Tls12))
                {
                    protocol |= Tls12;
                }

                ServicePointManager.SecurityProtocol = protocol;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed to set TLS security protocol.");
            }
        }

        public static void DisableTls12()
        {
            try
            {
                var protocol = ServicePointManager.SecurityProtocol;
                if (protocol.HasFlag(Tls12))
                {
                    Logger.Warn("Disabled Tls1.2 due to remote certificate error.");

                    ServicePointManager.SecurityProtocol = protocol & ~Tls12;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed to disable TLS 1.2 security protocol.");
            }
        }
    }
}
