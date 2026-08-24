using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Network;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host
{
    public static class ForwardedHeadersConfigurator
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(ForwardedHeadersConfigurator));

        public static void Configure(ForwardedHeadersOptions options, IConfigFileProvider configFileProvider)
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = null;

            var trustedNetworks = configFileProvider.TrustedNetworks;

            if (trustedNetworks.IsNullOrWhiteSpace())
            {
                return;
            }

            foreach (var entry in trustedNetworks.Split(','))
            {
                if (entry.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (IPNetworkParser.TryParse(entry, out var address, out var prefixLength))
                {
                    options.KnownNetworks.Add(new IPNetwork(address, prefixLength));

                    Logger.Info("Trusting forwarded headers from {0}/{1}", address, prefixLength);
                }
                else
                {
                    Logger.Warn("Invalid trusted network '{0}', forwarded headers from it will not be trusted", entry.Trim());
                }
            }
        }
    }
}
