using System;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DownloadClientCheck : IProvideHealthCheck
    {
        private readonly IProvideDownloadClient _downloadClientProvider;

        public DownloadClientCheck(IProvideDownloadClient downloadClientProvider)
        {
            _downloadClientProvider = downloadClientProvider;
        }

        public HealthCheck Check()
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (downloadClient == null)
            {
                return new HealthCheck(HealthCheckResultType.Warning, "No download client is available");
            }

            try
            {
                downloadClient.GetQueue();
            }
            catch (Exception)
            {
                return new HealthCheck(HealthCheckResultType.Error, "Unable to communicate with download client");
            }

            return null;
        }
    }
}
