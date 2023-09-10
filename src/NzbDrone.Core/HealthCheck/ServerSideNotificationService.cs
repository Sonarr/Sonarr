using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck
{
    public class ServerSideNotificationService : HealthCheckBase
    {
        private readonly IHttpClient _client;
        private readonly ISonarrCloudRequestBuilder _cloudRequestBuilder;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public ServerSideNotificationService(IHttpClient client, ISonarrCloudRequestBuilder cloudRequestBuilder, IConfigFileProvider configFileProvider, ILocalizationService localizationService, Logger logger)
            : base(localizationService)
        {
            _client = client;
            _cloudRequestBuilder = cloudRequestBuilder;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var request = _cloudRequestBuilder.Services.Create()
                .Resource("/notification")
                .AddQueryParam("version", BuildInfo.Version)
                .AddQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant())
                .AddQueryParam("arch", RuntimeInformation.OSArchitecture)
                .AddQueryParam("branch", _configFileProvider.Branch)
                .Build();

            try
            {
                _logger.Trace("Getting notifications");

                var response = _client.Execute(request);
                var result = Json.Deserialize<List<ServerNotificationResponse>>(response.Content);

                var checks = result.Select(x => new HealthCheck(GetType(), x.Type, x.Message, x.WikiUrl)).ToList();

                // Only one health check is supported, services returns an ordered list, so use the first one
                return checks.FirstOrDefault() ?? new HealthCheck(GetType());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to retrieve notifications");

                return new HealthCheck(GetType());
            }
        }
    }

    public class ServerNotificationResponse
    {
        public HealthCheckResult Type { get; set; }
        public string Message { get; set; }
        public string WikiUrl { get; set; }
    }
}
