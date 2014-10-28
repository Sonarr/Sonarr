using System;
using System.Linq;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DownloadClientCheck : HealthCheckBase
    {
        private readonly IProvideDownloadClient _downloadClientProvider;

        public DownloadClientCheck(IProvideDownloadClient downloadClientProvider)
        {
            _downloadClientProvider = downloadClientProvider;
        }

        public override HealthCheck Check()
        {
            var downloadClients = _downloadClientProvider.GetDownloadClients().ToList();

            if (!downloadClients.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "No download client is available");
            }

            try
            {
                foreach (var downloadClient in downloadClients)
                {
                    downloadClient.GetItems();
                }
            }
            catch (Exception e)
            {
               return new HealthCheck(GetType(), HealthCheckResult.Error, "Unable to communicate with download client " + e.Message);
            }

            return new HealthCheck(GetType());
        }
    }
}
