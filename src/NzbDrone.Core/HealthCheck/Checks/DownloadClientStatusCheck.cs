using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DownloadClientStatusCheck : HealthCheckBase
    {
        private readonly IDownloadClientFactory _providerFactory;
        private readonly IDownloadClientStatusService _providerStatusService;

        public DownloadClientStatusCheck(IDownloadClientFactory providerFactory, IDownloadClientStatusService providerStatusService)
        {
            _providerFactory = providerFactory;
            _providerStatusService = providerStatusService;
        }

        public override HealthCheck Check()
        {
            var enabledProviders = _providerFactory.GetAvailableProviders();
            var backOffProviders = enabledProviders.Join(_providerStatusService.GetBlockedProviders(),
                    i => i.Definition.Id,
                    s => s.ProviderId,
                    (i, s) => new { Provider = i, Status = s })
                .Where(v => (v.Status.MostRecentFailure - v.Status.InitialFailure) > TimeSpan.FromHours(1))
                .ToList();

            if (backOffProviders.Empty())
            {
                return new HealthCheck(GetType());
            }

            if (backOffProviders.Count == enabledProviders.Count)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "All download clients are unavailable due to failures", "#download-clients-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("Download clients unavailable due to failures: {0}", string.Join(", ", backOffProviders.Select(v => v.Provider.Definition.Name))), "#download-clients-are-unavailable-due-to-failures");
        }
    }
}
