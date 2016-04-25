using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NzbDrone.Common.Cloud;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class ProxyCheck : HealthCheckBase
    {
        private readonly Logger _logger;
        private readonly IConfigService _configService;
        private readonly IHttpClient _client;

        private readonly IHttpRequestBuilderFactory _cloudRequestBuilder;

        public ProxyCheck(ISonarrCloudRequestBuilder cloudRequestBuilder, IConfigService configService, IHttpClient client, Logger logger)
        {
            _configService = configService;
            _client = client;
            _logger = logger;

            _cloudRequestBuilder = cloudRequestBuilder.Services;
        }

        public override HealthCheck Check()
        {
            if (_configService.ProxyEnabled)
            {
                var addresses = Dns.GetHostAddresses(_configService.ProxyHostname);
                if(!addresses.Any())
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Failed to resolve the IP Address for the Configured Proxy Host {0}", _configService.ProxyHostname));
                }

                var request = _cloudRequestBuilder.Create()
                                                  .Resource("/ping")
                                                  .Build();

                try
                {
                    var response = _client.Execute(request);

                    // We only care about 400 responses, other error codes can be ignored 
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        _logger.Error("Proxy Health Check failed: {0}", response.StatusCode);
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Failed to test proxy: StatusCode {1}", request.Url, response.StatusCode));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Proxy Health Check failed: {0}", ex.Message);
                    return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Failed to test proxy: {1}", request.Url, ex.Message));
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
