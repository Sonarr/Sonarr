using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class ProxyCheck : HealthCheckBase
    {
        private readonly Logger _logger;
        private readonly IConfigService _configService;
        private readonly IHttpClient _client;

        public ProxyCheck(IConfigService configService, IHttpClient client, Logger logger)
        {
            _configService = configService;
            _client = client;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (_configService.ProxyEnabled)
            {
                var addresses = Dns.GetHostAddresses(_configService.ProxyHostname);
                if(!addresses.Any())
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error, "Failed to resolve the IP Address for the Configured Proxy Host:  " + _configService.ProxyHostname);
                }

                var request = new HttpRequestBuilder("https://services.sonarr.tv/ping").Build();

                try
                {
                    var response = _client.Execute(request);

                    // We only care about 400 responses, other error codes can be ignored 
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        _logger.Error("Proxy Health Check failed: {0}. Response Data: {1} ", response.StatusCode.ToString(), response.ResponseData);
                        return new HealthCheck(GetType(), HealthCheckResult.Error, "Failed to load https://sonarr.tv/, got HTTP " + response.StatusCode.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Proxy Health Check failed.", ex);
                    return new HealthCheck(GetType(), HealthCheckResult.Error, "An exception occured while trying to load https://sonarr.tv/: " + ex.Message);
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
