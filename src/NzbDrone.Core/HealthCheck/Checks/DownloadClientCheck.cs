using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<IDownloadClient>))]
    public class DownloadClientCheck : HealthCheckBase
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;

        public DownloadClientCheck(IProvideDownloadClient downloadClientProvider, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var downloadClients = _downloadClientProvider.GetDownloadClients().ToList();

            if (!downloadClients.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "No download client is available");
            }

            foreach (var downloadClient in downloadClients)
            {
                try
                {
                    downloadClient.GetItems();
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", downloadClient.Definition.Name);

                    var message = $"Unable to communicate with {downloadClient.Definition.Name}.";
                    return new HealthCheck(GetType(), HealthCheckResult.Error, $"{message} {ex.Message}", "#unable-to-communicate-with-download-client");
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
