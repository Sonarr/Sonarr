using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host
{
    public class ConfigureHostFilteringOptions : IConfigureOptions<HostFilteringOptions>
    {
        // Always allow localhost, loopback addresses and the hostname of the server, even if
        // they aren't configured in the allowed hosts list so users can access the UI using them.
        private static readonly string[] LoopbackHosts = { "localhost", "127.0.0.1", "[::1]" };

        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public ConfigureHostFilteringOptions(IConfigFileProvider configFileProvider, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void Configure(HostFilteringOptions options)
        {
            var allowedHosts = AllowedHostsParser.Parse(_configFileProvider.AllowedHosts);

            options.AllowEmptyHosts = true;

            if (allowedHosts.Empty())
            {
                options.AllowedHosts = new List<string> { "*" };

                _logger.Info("Allowed Hosts is not configured, accepting requests for any host");

                return;
            }

            options.AllowedHosts = allowedHosts.Concat(LoopbackHosts).Concat(GetServerHostNames()).Distinct().ToList();

            _logger.Info("Accepting requests for hosts: {0}", string.Join(", ", options.AllowedHosts));
        }

        private List<string> GetServerHostNames()
        {
            var hostNames = new List<string>();

            if (Environment.MachineName.IsNotNullOrWhiteSpace())
            {
                hostNames.Add(Environment.MachineName);
            }

            try
            {
                var hostName = Dns.GetHostName();

                if (hostName.IsNotNullOrWhiteSpace())
                {
                    hostNames.Add(hostName);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to get the hostname of the server");
            }

            return hostNames;
        }
    }
}
