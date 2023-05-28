using System;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class SystemTimeCheck : HealthCheckBase
    {
        private readonly IHttpClient _client;
        private readonly IHttpRequestBuilderFactory _cloudRequestBuilder;
        private readonly Logger _logger;

        public SystemTimeCheck(IHttpClient client, ISonarrCloudRequestBuilder cloudRequestBuilder, Logger logger, ILocalizationService localizationService)
            : base(localizationService)
        {
            _client = client;
            _cloudRequestBuilder = cloudRequestBuilder.Services;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var request = _cloudRequestBuilder.Create()
                .Resource("/time")
                .Build();

            var response = _client.Execute(request);
            var result = Json.Deserialize<ServiceTimeResponse>(response.Content);
            var systemTime = DateTime.UtcNow;

            // +/- more than 1 day
            if (Math.Abs(result.DateTimeUtc.Subtract(systemTime).TotalDays) >= 1)
            {
                _logger.Error("System time mismatch. SystemTime: {0} Expected Time: {1}. Update system time", systemTime, result.DateTimeUtc);
                return new HealthCheck(GetType(), HealthCheckResult.Error, _localizationService.GetLocalizedString("SystemTimeHealthCheckMessage"), "#system-time-off");
            }

            return new HealthCheck(GetType());
        }
    }

    public class ServiceTimeResponse
    {
        public DateTime DateTimeUtc { get; set; }
    }
}
