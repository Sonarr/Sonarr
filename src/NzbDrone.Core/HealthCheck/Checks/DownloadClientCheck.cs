using System;
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
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (downloadClient == null)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "No download client is available");
            }

            try
            {
                downloadClient.GetQueue();
            }
            catch (Exception)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "Unable to communicate with download client");
            }

            return new HealthCheck(GetType());
        }
    }
}
