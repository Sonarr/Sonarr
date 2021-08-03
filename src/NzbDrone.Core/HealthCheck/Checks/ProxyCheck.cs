using System;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
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
                if (!addresses.Any())
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
                        return new HealthCheck(GetType(), HealthCheckResult.Error, $"Failed to test proxy. StatusCode: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Proxy Health Check failed");
                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"Failed to test proxy: {request.Url}");
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
